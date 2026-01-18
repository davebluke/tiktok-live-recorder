import subprocess
import re
import os
import time
from datetime import datetime

# Regex to catch resolution from FFmpeg logs
# Matches pattern like "Stream #0:0: Video: h264, yuv420p, 1280x720,"
RESOLUTION_PATTERN = re.compile(r"Video:.*,\s*(\d{3,4}x\d{3,4})")

def record_stream(stream_url, output_file, ffmpeg_path="ffmpeg"):
    """
    Records the stream and restarts if resolution changes.
    Returns: 'FINISHED', 'RESTART', or 'ERROR'
    """
    print(f"[*] [SmartRecorder] Starting: {os.path.basename(output_file)}")
    
    # Dual-output magic: Copy to file + Decode to null (for logs)
    cmd = [
        ffmpeg_path, "-y", "-loglevel", "info", "-i", stream_url,
        "-map", "0", "-c", "copy", output_file,       # Main output
        "-map", "0:v", "-f", "null", "-"               # Log trigger
    ]

    process = None
    try:
        process = subprocess.Popen(
            cmd, stdout=subprocess.PIPE, stderr=subprocess.PIPE,
            universal_newlines=True, encoding="utf-8", errors="replace"
        )
    except Exception as e:
        print(f"[!] FFmpeg launch failed: {e}")
        return "ERROR"

    current_res = None
    
    try:
        while True:
            # Check for generic stops
            if process.poll() is not None:
                return "FINISHED"

            try:
                line = process.stderr.readline()
                
                if not line:
                    continue

                # Regex search
                match = RESOLUTION_PATTERN.search(line)
                if match:
                    new_res = match.group(1)
                    if current_res is None:
                        current_res = new_res
                        print(f"[*] Resolution: {current_res}")
                    elif new_res != current_res:
                        print(f"\n[!] Resolution Change: {current_res} -> {new_res}")
                        print("[!] Restarting session...")
                        process.terminate()
                        try:
                            process.wait(timeout=5)
                        except subprocess.TimeoutExpired:
                            process.kill()
                        return "RESTART"
            
            except UnicodeDecodeError:
                pass 

    except KeyboardInterrupt:
        print(f"\n[*] Gracefully stopping recording (CTRL+C received in SmartRecorder)...")
        if process:
            process.terminate()
            try:
                process.wait(timeout=5)
            except subprocess.TimeoutExpired:
                process.kill()
        return "MANUAL_STOP"
                
    except Exception as e:
        print(f"[!] Recorder Error: {e}")
        if process:
            try:
                process.terminate()
            except:
                process.kill()
        return "ERROR"
    
    return "FINISHED"