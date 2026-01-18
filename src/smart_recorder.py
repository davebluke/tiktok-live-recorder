import subprocess
import re
import os
import time
from datetime import datetime

# Regex to catch resolution from FFmpeg logs
# More permissive pattern to catch standard ffmpeg output
RESOLUTION_PATTERN = re.compile(r"Video:.* (\d{3,4}x\d{3,4})")

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

    process = subprocess.Popen(
        cmd, stdout=subprocess.PIPE, stderr=subprocess.PIPE,
        universal_newlines=True, encoding="utf-8", errors="replace"
    )

    current_res = None

    try:
        while True:
            try:
                line = process.stderr.readline()
                if not line and process.poll() is not None:
                    return "FINISHED"

                if line:
                    # Debug print to see what ffmpeg is outputting (optional, maybe distracting)
                    # print(f"[DEBUG] {line.strip()}") 
                    
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
                            except:
                                process.kill()
                            return "RESTART"
                            
            except KeyboardInterrupt:
                print(f"\n[*] Gracefully stopping recording...")
                process.terminate()
                try:
                    process.wait(timeout=5)
                except:
                    process.kill()
                return "MANUAL_STOP"
                
    except Exception as e:
        print(f"[!] Recorder Error: {e}")
        try:
            process.kill()
        except:
            pass
        return "ERROR"
    
    return "FINISHED"