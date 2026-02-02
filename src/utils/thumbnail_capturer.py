"""
Thumbnail Capturer for TikTok Live Recorder.

This module captures a single frame from a live stream and saves it
as a JPEG image in the status directory. Used by the GUI to display
live thumbnails for recording models.
"""

import os
import subprocess
import threading
import time
from typing import Optional


# Default status directory (same as status_manager.py)
DEFAULT_STATUS_DIR = ".tiktok_status"


class ThumbnailCapturer:
    """
    Captures thumbnails from a live stream using FFmpeg.
    Runs in a background thread to avoid blocking the recorder.
    """
    
    def __init__(self, username: str, stream_url: str, 
                 ffmpeg_path: str = "ffmpeg",
                 status_dir: str = DEFAULT_STATUS_DIR,
                 capture_interval: int = 60):
        """
        Initialize the thumbnail capturer.
        
        Args:
            username: TikTok username (used for filename)
            stream_url: URL of the live stream
            ffmpeg_path: Path to ffmpeg executable
            status_dir: Directory to save thumbnails
            capture_interval: Seconds between captures (default 60)
        """
        self.username = username
        self.stream_url = stream_url
        self.ffmpeg_path = ffmpeg_path
        self.status_dir = status_dir
        self.capture_interval = capture_interval
        self.output_path = os.path.join(status_dir, f"{username}.jpg")
        
        self._stop_event = threading.Event()
        self._thread: Optional[threading.Thread] = None
        
        # Ensure status directory exists
        os.makedirs(status_dir, exist_ok=True)
    
    def capture_once(self) -> bool:
        """
        Capture a single frame from the stream.
        
        Returns:
            True if capture succeeded, False otherwise.
        """
        # Use temp file to avoid partial writes
        temp_path = self.output_path + ".tmp"
        
        print(f"[*] [Thumbnail] Attempting capture for {self.username} -> {self.output_path}")
        
        cmd = [
            self.ffmpeg_path,
            "-y",  # Overwrite output
            "-loglevel", "warning",  # Show warnings too for debugging
            "-rw_timeout", "5000000",  # 5 second timeout
            "-i", self.stream_url,
            "-vframes", "1",  # Capture 1 frame
            "-q:v", "2",  # High quality JPEG
            "-vf", "scale=-1:400",  # Fixed height 400, preserve aspect ratio
            "-f", "image2",  # Explicitly specify image output format
            temp_path
        ]
        
        try:
            result = subprocess.run(
                cmd,
                capture_output=True,
                timeout=15,
                encoding="utf-8",
                errors="replace"
            )
            
            # Log any stderr output from FFmpeg
            if result.stderr:
                print(f"[!] [Thumbnail] FFmpeg output: {result.stderr[:300]}")
            
            if result.returncode != 0:
                print(f"[!] [Thumbnail] FFmpeg failed with code {result.returncode}")
                return False
            
            if os.path.exists(temp_path):
                # Check if file is valid (non-empty)
                file_size = os.path.getsize(temp_path)
                if file_size > 100:
                    # Atomic rename
                    if os.path.exists(self.output_path):
                        os.remove(self.output_path)
                    os.rename(temp_path, self.output_path)
                    print(f"[*] [Thumbnail] Captured: {self.username}.jpg ({file_size} bytes)")
                    return True
                else:
                    print(f"[!] [Thumbnail] File too small: {file_size} bytes")
                    os.remove(temp_path)
            else:
                print(f"[!] [Thumbnail] Temp file not created")
                    
        except subprocess.TimeoutExpired:
            print(f"[!] [Thumbnail] Capture timed out for {self.username}")
        except Exception as e:
            print(f"[!] [Thumbnail] Capture error for {self.username}: {e}")
        
        # Clean up temp file if it exists
        if os.path.exists(temp_path):
            try:
                os.remove(temp_path)
            except:
                pass
        
        return False
    
    def _capture_loop(self):
        """Background thread loop for periodic capture."""
        # Initial capture immediately
        self.capture_once()
        
        while not self._stop_event.is_set():
            # Wait for interval or stop signal
            if self._stop_event.wait(timeout=self.capture_interval):
                break  # Stop signal received
            
            # Capture thumbnail
            self.capture_once()
    
    def start(self):
        """Start background thumbnail capture thread."""
        if self._thread is not None and self._thread.is_alive():
            return  # Already running
        
        self._stop_event.clear()
        self._thread = threading.Thread(target=self._capture_loop, daemon=True)
        self._thread.start()
        print(f"[*] [Thumbnail] Started capture thread for {self.username}")
    
    def stop(self):
        """Stop the background capture thread."""
        self._stop_event.set()
        if self._thread is not None and self._thread.is_alive():
            self._thread.join(timeout=2)
        print(f"[*] [Thumbnail] Stopped capture thread for {self.username}")
    
    def cleanup(self):
        """Remove the thumbnail file."""
        self.stop()
        try:
            if os.path.exists(self.output_path):
                os.remove(self.output_path)
        except Exception:
            pass


def capture_thumbnail(stream_url: str, username: str, 
                      ffmpeg_path: str = "ffmpeg",
                      status_dir: str = DEFAULT_STATUS_DIR) -> bool:
    """
    Convenience function to capture a single thumbnail.
    
    Args:
        stream_url: URL of the live stream
        username: TikTok username (used for filename)
        ffmpeg_path: Path to ffmpeg executable
        status_dir: Directory to save thumbnail
        
    Returns:
        True if capture succeeded, False otherwise.
    """
    capturer = ThumbnailCapturer(username, stream_url, ffmpeg_path, status_dir)
    return capturer.capture_once()
