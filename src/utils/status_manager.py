"""
Status Manager for TikTok Live Recorder Multi-Instance Monitoring.

This module provides a thread-safe status file manager that enables
multiple recorder instances to broadcast their state to a central
monitoring dashboard.

Status files are JSON files stored in .tiktok_status/ directory,
one per running instance (named {username}.json).
"""

import os
import json
import time
import atexit
import threading
from datetime import datetime
from typing import Optional


# Default status directory (relative to working directory)
DEFAULT_STATUS_DIR = ".tiktok_status"


class StatusManager:
    """
    Manages status file for a single recorder instance.
    
    Thread-safe operations for writing status updates to a JSON file.
    Automatically cleans up status file on exit.
    """
    
    # Status states
    STATE_STARTING = "STARTING"
    STATE_WAITING = "WAITING"
    STATE_RECORDING = "RECORDING"
    STATE_STOPPED = "STOPPED"
    
    def __init__(self, username: str, status_dir: str = DEFAULT_STATUS_DIR):
        """
        Initialize the StatusManager for a specific user.
        
        Args:
            username: TikTok username being monitored/recorded
            status_dir: Directory to store status files
        """
        self.username = username
        self.status_dir = status_dir
        self.status_file = os.path.join(status_dir, f"{username}.json")
        self.pid = os.getpid()
        self.started_at = datetime.now().isoformat()
        self.current_file: Optional[str] = None
        self.file_size_mb: float = 0.0
        self._lock = threading.Lock()
        self._state = self.STATE_STARTING
        
        # Ensure status directory exists
        os.makedirs(status_dir, exist_ok=True)
        
        # Register cleanup on exit
        atexit.register(self._cleanup)
        
        # Write initial status
        self._write_status()
    
    def _write_status(self) -> None:
        """Write current status to the JSON file (thread-safe)."""
        with self._lock:
            status_data = {
                "username": self.username,
                "state": self._state,
                "pid": self.pid,
                "started_at": self.started_at,
                "last_heartbeat": datetime.now().isoformat(),
                "current_file": self.current_file,
                "file_size_mb": round(self.file_size_mb, 2)
            }
            
            try:
                # Write atomically using temp file + rename
                temp_file = self.status_file + ".tmp"
                with open(temp_file, "w", encoding="utf-8") as f:
                    json.dump(status_data, f, indent=2)
                
                # Atomic rename (works on Windows with same drive)
                if os.path.exists(self.status_file):
                    os.remove(self.status_file)
                os.rename(temp_file, self.status_file)
                
            except Exception as e:
                # Non-critical - log but don't crash
                print(f"[StatusManager] Warning: Could not write status: {e}")
    
    def set_state(self, state: str) -> None:
        """
        Update the current state.
        
        Args:
            state: One of STATE_STARTING, STATE_WAITING, STATE_RECORDING, STATE_STOPPED
        """
        self._state = state
        self._write_status()
    
    def set_waiting(self) -> None:
        """Set state to WAITING (user offline)."""
        self.current_file = None
        self.file_size_mb = 0.0
        self.set_state(self.STATE_WAITING)
    
    def set_recording(self, filename: str) -> None:
        """
        Set state to RECORDING.
        
        Args:
            filename: Name of the file being recorded
        """
        self.current_file = filename
        self.file_size_mb = 0.0
        self.set_state(self.STATE_RECORDING)
    
    def update_recording_progress(self, file_size_mb: float) -> None:
        """
        Update recording progress (file size).
        
        Args:
            file_size_mb: Current file size in megabytes
        """
        self.file_size_mb = file_size_mb
        self._write_status()
    
    def heartbeat(self) -> None:
        """Update the heartbeat timestamp without changing state."""
        self._write_status()
    
    def set_stopped(self) -> None:
        """Set state to STOPPED (clean shutdown)."""
        self.current_file = None
        self.set_state(self.STATE_STOPPED)
    
    def _cleanup(self) -> None:
        """Remove status file on exit."""
        try:
            if os.path.exists(self.status_file):
                os.remove(self.status_file)
        except Exception:
            pass  # Best effort cleanup


def get_all_statuses(status_dir: str = DEFAULT_STATUS_DIR) -> list:
    """
    Read all status files from the status directory.
    
    Args:
        status_dir: Directory containing status files
        
    Returns:
        List of status dictionaries, sorted by username
    """
    statuses = []
    
    if not os.path.exists(status_dir):
        return statuses
    
    for filename in os.listdir(status_dir):
        if filename.endswith(".json") and not filename.endswith(".tmp"):
            filepath = os.path.join(status_dir, filename)
            try:
                with open(filepath, "r", encoding="utf-8") as f:
                    status = json.load(f)
                    
                    # Check for stale status (no heartbeat in 60 seconds)
                    last_heartbeat = datetime.fromisoformat(status.get("last_heartbeat", ""))
                    age_seconds = (datetime.now() - last_heartbeat).total_seconds()
                    status["is_stale"] = age_seconds > 60
                    status["age_seconds"] = round(age_seconds)
                    
                    statuses.append(status)
            except Exception:
                # Skip corrupted or locked files
                pass
    
    # Sort by username
    statuses.sort(key=lambda x: x.get("username", ""))
    return statuses
