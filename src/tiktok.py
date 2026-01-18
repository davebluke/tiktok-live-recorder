import os
import time
import logging
import requests
import re
from datetime import datetime

# --- NEW IMPORT FOR RESOLUTION DETECTION ---
try:
    from src.smart_recorder import record_stream
except ImportError:
    # Fallback if running from root without package context
    from smart_recorder import record_stream
# -------------------------------------------

class TikTok:
    def __init__(self, output, mode, user, ffmpeg="ffmpeg", interval=5, update_check=True):
        self.output = output
        self.mode = mode
        self.user = user
        self.ffmpeg = ffmpeg
        self.interval = interval
        self.update_check = update_check
        
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
        """
        try:
            url = f"https://webcast.tiktok.com/webcast/room/info/?aid=1988&room_id={room_id}"
            response = requests.get(url, headers=self.headers).json()
            
            # Extract stream URL
            if "data" in response:
                data = response["data"]
                
                # Check live status (2 = Live, 4 = Finish/Offline)
                status = data.get("status")
                if status != 2:
                    return None

                if "stream_url" in data:
                    stream_info = data["stream_url"]
                
                # Priority: FLV Pull URL -> RTMP Pull URL
                if "flv_pull_url" in stream_info:
                    return stream_info["flv_pull_url"].get("FULL_HD1") or \
                           stream_info["flv_pull_url"].get("HD1") or \
                           stream_info["flv_pull_url"].get("SD1")
                
                if "rtmp_pull_url" in stream_info:
                    return stream_info["rtmp_pull_url"]
                    
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
        filename = f"{self.user}_{current_date}.mp4"
        output_path = os.path.join(self.output, filename)

        print(f"\n[*] [TikTok] Recording started for {self.user}")
        print(f"[*] [TikTok] Output: {output_path}")

        # --- SMART RECORDING LOOP ---
        while True:
            # Pass execution to the smart recorder module
            # status will be: "FINISHED", "RESTART", or "ERROR"
            status = record_stream(stream_url, output_path, self.ffmpeg)
            
            if status == "RESTART":
                # Resolution changed!
                print(f"[*] [TikTok] Restarting recording session due to resolution change...")
                
                # Create a new filename for the next part
                # Format: user_Date_Time_Part2.mp4
                timestamp = datetime.now().strftime("%H-%M-%S")
                base_name = f"{self.user}_{current_date}_{timestamp}.mp4"
                output_path = os.path.join(self.output, base_name)
                
                # Small buffer to let the OS release file locks
                time.sleep(1)
                continue 
            
            elif status == "FINISHED":
                print(f"[*] [TikTok] Stream ended naturally.")
                break
            
            elif status == "ERROR":
                print(f"[!] [TikTok] An error occurred while recording.")
                break
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
                        print(f"[*] {self.user} is LIVE! (Room ID: {room_id})")
                        self.start_recording(stream_url)
                    else:
                        print(f"[*] {self.user} is offline. Checking again...", end="\r")
                else:
                    print(f"[*] {self.user} is offline. Checking again...", end="\r")

                if self.mode == "manual":
                    break
                
                # Wait before checking again (Automatic mode)
                time.sleep(self.interval * 60)

            except KeyboardInterrupt:
                print("\n[*] Stopped by user.")
                break
            except Exception as e:
                print(f"\n[!] Unexpected Error: {e}")
                time.sleep(10)