import os
import time
import logging
import requests
import re
from datetime import datetime
import colorama

# Initialize colorama for Windows support
colorama.init()

# --- NEW IMPORT FOR RESOLUTION DETECTION ---
try:
    from src.smart_recorder import record_stream
except ImportError:
    # Fallback if running from root without package context
    from smart_recorder import record_stream

# --- STATUS MANAGER FOR MULTI-INSTANCE MONITORING ---
try:
    from src.utils.status_manager import StatusManager
except ImportError:
    try:
        from utils.status_manager import StatusManager
    except ImportError:
        # Fallback: create a dummy StatusManager if not available
        class StatusManager:
            def __init__(self, *args, **kwargs): pass
            def set_waiting(self): pass
            def set_recording(self, filename): pass
            def update_recording_progress(self, size): pass
            def heartbeat(self): pass
            def set_stopped(self): pass

# -------------------------------------------

# ANSI Colors
GREEN = "\033[92m"
RED = "\033[91m"
RESET = "\033[0m"

class TikTok:
    def __init__(self, output, mode, user, ffmpeg="ffmpeg", interval=5, update_check=True):
        self.output = output
        self.mode = mode
        self.user = user
        self.ffmpeg = ffmpeg
        self.interval = interval
        self.update_check = update_check
        
        # Initialize status manager for multi-instance monitoring
        self.status_manager = StatusManager(user)
        
        # Headers mimicking a real browser to avoid detection
        self.headers = {
            "User-Agent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36",
            "Referer": "https://www.tiktok.com/",
        }
        
        # Ensure output directory exists
        if not os.path.exists(self.output):
            os.makedirs(self.output)

    def get_room_id(self):
        """
        Retrieves the Room ID for the given user. 
        Returns None if user is not found or not live.
        """
        try:
            url = f"https://www.tiktok.com/@{self.user}/live"
            response = requests.get(url, headers=self.headers, allow_redirects=False)
            
            # If we get a redirect, the room might be offline or user doesn't exist
            if response.status_code == 302:
                return None

            content = response.text
            
            # Regex to find roomId in the HTML
            if "roomId" in content:
                room_id = re.search(r'"roomId":"(\d+)"', content)
                if room_id:
                    return room_id.group(1)
            
            # Alternative regex pattern common in TikTok source
            room_id = re.search(r'room_id=(\d+)', content)
            if room_id:
                return room_id.group(1)

        except Exception as e:
            logging.error(f"Error retrieving Room ID: {e}")
            
        return None

    def get_stream_url(self, room_id):
        """
        Fetches the FLV stream URL using the Room ID.
        Uses live_core_sdk_data for highest quality stream selection (like original repo).
        """
        import json as json_module
        
        try:
            url = f"https://webcast.tiktok.com/webcast/room/info/?aid=1988&room_id={room_id}"
            response = requests.get(url, headers=self.headers).json()
            
            # Extract stream URL
            if "data" in response:
                data = response["data"]
                
                # Check live status (2 = Live, 4 = Finish/Offline)
                status = data.get("status")
                if status != 2:
                    timestamp = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
                    print(f"[!] {timestamp} - Stream status is {status}, not live (2)")
                    return None

                if "stream_url" not in data:
                    print("[!] No stream_url in API response")
                    return None
                    
                stream_info = data["stream_url"]
                
                # --- PRIORITY 1: Use live_core_sdk_data for highest quality (like original repo) ---
                try:
                    sdk_data_str = (
                        stream_info.get("live_core_sdk_data", {})
                        .get("pull_data", {})
                        .get("stream_data")
                    )
                    
                    if sdk_data_str:
                        sdk_data = json_module.loads(sdk_data_str).get("data", {})
                        
                        # Get quality levels
                        qualities = (
                            stream_info.get("live_core_sdk_data", {})
                            .get("pull_data", {})
                            .get("options", {})
                            .get("qualities", [])
                        )
                        
                        if qualities:
                            level_map = {q["sdk_key"]: q["level"] for q in qualities}
                            print(f"[*] SDK Qualities available: {list(level_map.keys())}")
                            
                            best_level = -1
                            best_flv = None
                            best_quality_name = None
                            
                            for sdk_key, entry in sdk_data.items():
                                level = level_map.get(sdk_key, -1)
                                stream_main = entry.get("main", {})
                                flv_url = stream_main.get("flv")
                                
                                if level > best_level and flv_url:
                                    best_level = level
                                    best_flv = flv_url
                                    best_quality_name = sdk_key
                            
                            if best_flv:
                                print(f"[*] Using SDK stream: {best_quality_name} (level {best_level})")
                                return best_flv
                            else:
                                print("[!] SDK stream data found but no FLV URL extracted")
                        else:
                            print("[!] No qualities found in SDK stream data")
                    else:
                        print("[*] No SDK stream data, falling back to legacy URLs...")
                        
                except Exception as sdk_error:
                    print(f"[!] SDK extraction failed: {sdk_error}, falling back to legacy URLs...")
                
                # --- PRIORITY 2: Fallback to legacy flv_pull_url (with quality preference) ---
                if "flv_pull_url" in stream_info:
                    flv_urls = stream_info["flv_pull_url"]
                    print(f"[*] Legacy qualities available: {list(flv_urls.keys())}")
                    
                    # Try quality levels in order of preference
                    for quality in ["FULL_HD1", "HD1", "ORIGIN", "SD2", "SD1"]:
                        if quality in flv_urls and flv_urls[quality]:
                            print(f"[*] Using legacy stream quality: {quality}")
                            return flv_urls[quality]
                    
                    # If no known quality, try the first available
                    if flv_urls:
                        first_key = list(flv_urls.keys())[0]
                        print(f"[*] Using fallback legacy stream: {first_key}")
                        return flv_urls[first_key]
                
                if "rtmp_pull_url" in stream_info:
                    print("[*] Using RTMP pull URL")
                    return stream_info["rtmp_pull_url"]
                    
                # Check for HLS as fallback
                if "hls_pull_url" in stream_info:
                    print("[*] Using HLS pull URL")
                    return stream_info["hls_pull_url"]
                    
                print(f"[!] No usable stream URL found. Available keys: {list(stream_info.keys())}")
                    
        except Exception as e:
            logging.error(f"Error retrieving Stream URL: {e}")
            
        return None

    def is_live(self):
        """
        Checks if the user is currently live.
        """
        room_id = self.get_room_id()
        if room_id:
            return room_id, True
        return None, False

    def start_recording(self, stream_url):
        """
        Starts the recording process using the Smart Recorder.
        Monitors for resolution changes (e.g. PK Battles) and restarts automatically.
        """
        current_date = datetime.now().strftime("%Y.%m.%d_%H-%M-%S")
        filename = f"v02__{self.user}_{current_date}.mp4"
        output_path = os.path.join(self.output, filename)

        print(f"\n[*] [TikTok] Recording started for {self.user}")
        print(f"[*] [TikTok] Output: {output_path}")

        # --- SMART RECORDING LOOP ---
        # --- SMART RECORDING LOOP ---
        try:
            while True:
                # Use a temp filename while recording
                # e.g. user_date.mp4 -> user_date_flv.mp4
                if filename.endswith(".mp4"):
                    temp_filename = filename.replace(".mp4", "_flv.mp4")
                else:
                    temp_filename = f"{filename}_flv.mp4"
                
                temp_path = os.path.join(self.output, temp_filename)
                
                # Pass execution to the smart recorder module
                # status will be: "FINISHED", "RESTART", "ERROR", or "MANUAL_STOP"
                status = record_stream(stream_url, temp_path, self.ffmpeg)
                
                # The conversion already renamed the file (from _flv.mp4 to .mp4)
                # so the final path is without the _flv suffix
                final_path = temp_path.replace("_flv.mp4", ".mp4")
                if os.path.exists(final_path):
                    print(f"[*] [TikTok] Recording saved: {final_path}")

                if status == "RESTART":
                    # Resolution changed!
                    print(f"[*] [TikTok] Restarting recording session due to resolution change...")
                    
                    # Create a new filename for the next part
                    # Format: user_Date_Time_Part2.mp4
                    timestamp = datetime.now().strftime("%H-%M-%S")
                    base_name = f"v02__{self.user}_{current_date}_{timestamp}.mp4"
                    filename = base_name # Update filename for next loop
                    output_path = os.path.join(self.output, base_name)
                    
                    # Small buffer to let the OS release file locks
                    time.sleep(1)
                    continue 
                
                elif status == "FINISHED":
                    print(f"[*] [TikTok] Stream ended naturally.")
                    break
                
                elif status == "MANUAL_STOP":
                    print(f"[*] [TikTok] Recording stopped by user.")
                    break
                
                elif status == "ERROR":
                    print(f"[!] [TikTok] An error occurred while recording.")
                    break
                    
        except KeyboardInterrupt:
             # Just in case it bubbles up here
             return "MANUAL_STOP"
        
        # If we exit loop naturally or via non-manual stop logic (though loop handles most)
        if 'status' in locals() and status == "MANUAL_STOP":
            return "MANUAL_STOP"
            
        return "FINISHED"
        # ----------------------------

    def run(self):
        """
        Main loop handling the 'automatic' or 'manual' modes.
        """
        print(f"[*] Target User: {self.user}")
        print(f"[*] Mode: {self.mode}")
        
        while True:
            try:
                room_id = self.get_room_id()
                
                if room_id:
                    # Check if actually live via API
                    stream_url = self.get_stream_url(room_id)
                    
                    if stream_url:
                        print(f"[*] {GREEN}{self.user} is LIVE!{RESET} (Room ID: {room_id})")
                        # Update status to RECORDING before starting
                        current_date = datetime.now().strftime("%Y.%m.%d_%H-%M-%S")
                        self.status_manager.set_recording(f"v02__{self.user}_{current_date}.mp4")
                        status = self.start_recording(stream_url)
                        
                        # Handle different end statuses
                        if status == "MANUAL_STOP":
                             print(f"[*] Manual stop detected. Resuming monitoring in 3 seconds...")
                             time.sleep(3)
                             continue
                        
                        elif status == "FINISHED":
                            # Stream ended naturally - check again soon
                            if self.mode == "automatic":
                                print(f"\n[*] {self.user} went offline. Waiting 30 seconds before checking again...")
                                time.sleep(30)
                                continue
                            else:
                                print(f"[*] Stream ended. Mode is manual, exiting.")
                                break
                        
                        elif status == "ERROR":
                            # Error occurred - wait a bit and retry
                            if self.mode == "automatic":
                                print(f"\n[!] Error occurred. Retrying in 30 seconds...")
                                time.sleep(30)
                                continue
                    else:
                        timestamp = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
                        print(f"[*] {timestamp} - {RED}{self.user} is offline.{RESET} Checking again...", end="\r")
                        self.status_manager.set_waiting()
                else:
                    timestamp = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
                    print(f"[*] {timestamp} - {RED}{self.user} is offline.{RESET} Checking again...", end="\r")
                    self.status_manager.set_waiting()

                if self.mode == "manual":
                    break
                
                # Wait before checking again (Automatic mode)
                try:
                    time.sleep(self.interval * 60)
                except KeyboardInterrupt:
                    # If CTRL+C during sleep
                    print("\n[*] Stopped by user during wait.")
                    break

            except KeyboardInterrupt:
                print("\n[*] Stopped by user.")
                self.status_manager.set_stopped()
                break
            except Exception as e:
                print(f"\n[!] Unexpected Error: {e}")
                time.sleep(10)