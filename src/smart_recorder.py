import subprocess
import re
import os
import sys
import time
import threading
from datetime import datetime

# Windows keyboard detection
try:
    import msvcrt
    HAS_MSVCRT = True
except ImportError:
    HAS_MSVCRT = False

from utils.video_management import VideoManagement

# Regex to catch resolution from FFprobe JSON output
RESOLUTION_PATTERN = re.compile(r'"width"\s*:\s*(\d+).*?"height"\s*:\s*(\d+)', re.DOTALL)


class ResolutionMonitor:
    """
    Monitors a stream URL for resolution changes using ffprobe.
    Runs in a separate thread and sets a flag when resolution changes.
    """
    
    def __init__(self, stream_url, ffprobe_path="ffprobe", poll_interval=3, stability_threshold=2):
        self.stream_url = stream_url
        self.ffprobe_path = ffprobe_path
        self.poll_interval = poll_interval
        self.stability_threshold = stability_threshold  # Number of consistent readings required to confirm change
        
        self.current_resolution = None
        self.resolution_changed = threading.Event()
        self.new_resolution = None
        self.stop_event = threading.Event()
        self._thread = None
        
        # Stability tracking
        self.pending_resolution = None
        self.consistency_count = 0
    
    def get_stream_resolution(self):
        """
        Uses ffprobe to get the current resolution of the stream.
        Returns tuple (width, height) or None if failed.
        """
        cmd = [
            self.ffprobe_path,
            "-v", "error",
            "-select_streams", "v:0",
            "-show_entries", "stream=width,height",
            "-of", "json",
            "-timeout", "10000000",  # 10 second timeout in microseconds
            self.stream_url
        ]
        
        try:
            result = subprocess.run(
                cmd,
                capture_output=True,
                text=True,
                timeout=15,
                encoding="utf-8",
                errors="replace"
            )
            
            if result.returncode == 0:
                match = RESOLUTION_PATTERN.search(result.stdout)
                if match:
                    width = int(match.group(1))
                    height = int(match.group(2))
                    return (width, height)
        except subprocess.TimeoutExpired:
            pass
        except Exception as e:
            # Don't print every error to avoid spamming console
            pass
        
        return None
    
    def _monitor_loop(self):
        """
        Main monitoring loop that runs in a thread.
        Polls the stream resolution and checks for changes.
        """
        display_counter = 0
        display_interval = 60 // self.poll_interval  # Show resolution roughly every minute
        
        while not self.stop_event.is_set():
            resolution = self.get_stream_resolution()
            
            if resolution:
                if self.current_resolution is None:
                    # First detection - accept immediately
                    self.current_resolution = resolution
                    print(f"[*] Recording Resolution: {resolution[0]}x{resolution[1]}")
                
                elif resolution != self.current_resolution:
                    # Potential change detected!
                    if resolution == self.pending_resolution:
                        self.consistency_count += 1
                        print(f"[*] Verifying resolution change ({self.consistency_count}/{self.stability_threshold})...")
                    else:
                        # New potential change detected
                        self.pending_resolution = resolution
                        self.consistency_count = 1
                    
                    # If we have enough consistent readings, confirm the change
                    if self.consistency_count >= self.stability_threshold:
                        old_res = self.current_resolution
                        self.new_resolution = resolution
                        print(f"\n[!] Resolution Change Confirmed: {old_res[0]}x{old_res[1]} -> {resolution[0]}x{resolution[1]}")
                        self.resolution_changed.set()
                        return  # Exit the monitor loop
                
                else:
                    # Resolution matches current - reset consistency checks
                    self.pending_resolution = None
                    self.consistency_count = 0
                    
                    # Periodically display current resolution
                    display_counter += 1
                    if display_counter >= display_interval:
                        print(f"[*] Current Resolution: {resolution[0]}x{resolution[1]} (Stable)")
                        display_counter = 0
            
            # Wait for poll interval or until stopped
            self.stop_event.wait(timeout=self.poll_interval)
    
    def start(self):
        """Start the resolution monitoring thread."""
        self._thread = threading.Thread(target=self._monitor_loop, daemon=True)
        self._thread.start()
    
    def stop(self):
        """Stop the resolution monitoring thread."""
        self.stop_event.set()
        if self._thread and self._thread.is_alive():
            self._thread.join(timeout=2)
    
    def has_changed(self):
        """Check if resolution has changed."""
        return self.resolution_changed.is_set()


def record_stream(stream_url, output_file, ffmpeg_path="ffmpeg", status_manager=None):
    """
    Records the stream and restarts if resolution changes.
    Returns: 'FINISHED', 'RESTART', 'ERROR', or 'MANUAL_STOP'
    
    Args:
        stream_url: URL of the stream to record
        output_file: Path to save the recording
        ffmpeg_path: Path to ffmpeg executable
        status_manager: Optional StatusManager for heartbeat updates
    """
    print(f"[*] [SmartRecorder] Starting: {os.path.basename(output_file)}")
    
    # Derive ffprobe path from ffmpeg path
    if ffmpeg_path.endswith("ffmpeg") or ffmpeg_path.endswith("ffmpeg.exe"):
        ffprobe_path = ffmpeg_path.replace("ffmpeg", "ffprobe")
    else:
        ffprobe_path = "ffprobe"
    
    # Start resolution monitor
    monitor = ResolutionMonitor(stream_url, ffprobe_path=ffprobe_path, poll_interval=3)
    monitor.start()
    
    # Wait briefly for initial resolution detection
    time.sleep(1)
    
    # Simple FFmpeg command - just record, no dual-output needed
    cmd = [
        ffmpeg_path, "-y", "-loglevel", "info",
        "-rw_timeout", "10000000",  # 10 second read/write timeout
        "-i", stream_url,
        "-c", "copy",
        output_file
    ]
    
    print(f"[*] [SmartRecorder] Stream URL: {stream_url[:100]}...")  # Debug: show stream URL

    process = None
    try:
        process = subprocess.Popen(
            cmd, 
            stdin=subprocess.PIPE,  # Allow sending commands like 'q'
            stdout=subprocess.PIPE, 
            stderr=subprocess.PIPE,
            universal_newlines=True, 
            encoding="utf-8", 
            errors="replace"
        )
    except Exception as e:
        print(f"[!] FFmpeg launch failed: {e}")
        monitor.stop()
        return "ERROR"

    def convert_and_return(status):
        """Helper to convert FLV to MP4 before returning status."""
        if os.path.exists(output_file) and os.path.getsize(output_file) > 0:
            VideoManagement.convert_flv_to_mp4(output_file)
        else:
            print(f"[!] Output file empty or missing: {output_file}")
        return status

    # ANSI codes for formatting
    BOLD = "\033[1m"
    RESET = "\033[0m"
    
    def format_ffmpeg_line(line):
        """Format FFmpeg progress line with MB size and alternating bold."""
        # Convert size from kB/KiB to MB
        # FFmpeg may output size= in kB (1000 bytes) or KiB (1024 bytes)
        size_match = re.search(r'size=\s*(\d+)\s*(kB|KiB)', line, re.IGNORECASE)
        if size_match:
            size_value = int(size_match.group(1))
            unit = size_match.group(2).lower()
            # Convert to MB (using 1024 for both since FFmpeg's kB is actually KiB)
            mb_value = size_value / 1024
            line = re.sub(r'size=\s*\d+\s*(kB|KiB)', f'size={mb_value:.2f}MB', line, flags=re.IGNORECASE)
        
        # Parse the line into key=value pairs
        # Pattern: frame=  123 fps= 21 q=-1.0 size=4.25MB time=00:00:28.12 bitrate=1267.9kbits/s speed=1.42x
        parts = re.findall(r'(\w+)=\s*([^\s]+)', line)
        
        if parts:
            formatted_parts = []
            for i, (key, value) in enumerate(parts):
                if i % 2 == 0:  # Bold: frame, q, time, speed
                    formatted_parts.append(f"{BOLD}{key}={value}{RESET}")
                else:  # Regular: fps, size, bitrate
                    formatted_parts.append(f"{key}={value}")
            return "[FFmpeg] " + "  ".join(formatted_parts)
        
        return f"[FFmpeg] {line}"
    
    # Thread to read FFmpeg stderr continuously
    stderr_output = []
    def read_stderr():
        try:
            for line in process.stderr:
                line = line.strip()
                if line:
                    stderr_output.append(line)
                    # Print progress info (lines with time= or speed=)
                    if "time=" in line:
                        print(format_ffmpeg_line(line))
                    elif "error" in line.lower():
                        print(f"[FFmpeg] {line[:150]}")
        except:
            pass
    
    stderr_thread = threading.Thread(target=read_stderr, daemon=True)
    stderr_thread.start()

    try:
        while True:
            # Check if FFmpeg has stopped
            exit_code = process.poll()
            if exit_code is not None:
                monitor.stop()
                if exit_code == 0:
                    return convert_and_return("FINISHED")
                else:
                    # Read any error output
                    stderr_output = process.stderr.read() if process.stderr else ""
                    if stderr_output:
                        print(f"[!] FFmpeg stderr: {stderr_output[:500]}")
                    return convert_and_return("FINISHED")  # Stream probably ended
            
            # Check for resolution change
            if monitor.has_changed():
                print("[!] Restarting session due to resolution change...")
                try:
                    process.stdin.write("q")
                    process.stdin.flush()
                    process.wait(timeout=5)
                except (IOError, ValueError, subprocess.TimeoutExpired):
                    process.terminate()
                    try:
                        process.wait(timeout=5)
                    except subprocess.TimeoutExpired:
                        process.kill()
                monitor.stop()
                return convert_and_return("RESTART")
            
            # Check for 'q' key press (Windows only)
            if HAS_MSVCRT and msvcrt.kbhit():
                key = msvcrt.getch()
                if key in (b'q', b'Q'):
                    print("\n[*] 'q' pressed - Gracefully stopping recording...")
                    monitor.stop()
                    try:
                        process.stdin.write("q")
                        process.stdin.flush()
                        process.wait(timeout=5)
                    except (IOError, ValueError, subprocess.TimeoutExpired):
                        process.terminate()
                        try:
                            process.wait(timeout=5)
                        except subprocess.TimeoutExpired:
                            process.kill()
                    return convert_and_return("MANUAL_STOP")
            
            # Update status manager heartbeat and file size
            if status_manager:
                try:
                    if os.path.exists(output_file):
                        file_size_mb = os.path.getsize(output_file) / (1024 * 1024)
                        status_manager.update_recording_progress(file_size_mb)
                    else:
                        status_manager.heartbeat()
                except Exception:
                    pass  # Non-critical, don't crash recording
            
            # Small sleep to prevent busy-waiting
            time.sleep(0.5)

    except KeyboardInterrupt:
        print(f"\n[*] Gracefully stopping recording (CTRL+C received)...")
        monitor.stop()
        if process:
            try:
                # Send 'q' to quit gracefully and allow MP4 to finalize
                process.stdin.write("q")
                process.stdin.flush()
                process.wait(timeout=5)
            except (IOError, ValueError, subprocess.TimeoutExpired):
                # If 'q' fails or times out, force terminate
                process.terminate()
                try:
                    process.wait(timeout=5)
                except subprocess.TimeoutExpired:
                    process.kill()
        return convert_and_return("MANUAL_STOP")
                
    except Exception as e:
        print(f"[!] Recorder Error: {e}")
        monitor.stop()
        if process:
            try:
                process.terminate()
            except:
                process.kill()
        return "ERROR"
    
    return "FINISHED"