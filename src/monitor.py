#!/usr/bin/env python3
"""
TikTok Live Recorder - Multi-Instance Monitor Dashboard

This script provides a real-time dashboard for monitoring multiple
TikTok live recorder instances running concurrently.

Usage:
    python monitor.py [--status-dir .tiktok_status] [--refresh 2]

The dashboard displays:
    - Username being monitored
    - Current state (RECORDING, WAITING, etc.)
    - Time since last heartbeat
    - Current file being recorded and its size
"""

import os
import sys
import time
import shutil
import argparse
from datetime import datetime, timedelta

# Try to import rich for beautiful terminal output
try:
    from rich.console import Console
    from rich.table import Table
    from rich.live import Live
    from rich.panel import Panel
    from rich.text import Text
    HAS_RICH = True
except ImportError:
    HAS_RICH = False

# Add src directory to path for imports
current_dir = os.path.dirname(os.path.abspath(__file__))
if current_dir not in sys.path:
    sys.path.insert(0, current_dir)

try:
    from utils.status_manager import get_all_statuses, DEFAULT_STATUS_DIR
except ImportError:
    try:
        from src.utils.status_manager import get_all_statuses, DEFAULT_STATUS_DIR
    except ImportError:
        # Inline implementation if module not found
        import json
        DEFAULT_STATUS_DIR = ".tiktok_status"
        
        def get_all_statuses(status_dir=DEFAULT_STATUS_DIR):
            statuses = []
            if not os.path.exists(status_dir):
                return statuses
            for filename in os.listdir(status_dir):
                if filename.endswith(".json") and not filename.endswith(".tmp"):
                    filepath = os.path.join(status_dir, filename)
                    try:
                        with open(filepath, "r", encoding="utf-8") as f:
                            status = json.load(f)
                            last_heartbeat = datetime.fromisoformat(status.get("last_heartbeat", ""))
                            age_seconds = (datetime.now() - last_heartbeat).total_seconds()
                            status["is_stale"] = age_seconds > 60
                            status["age_seconds"] = round(age_seconds)
                            statuses.append(status)
                    except Exception:
                        pass
            statuses.sort(key=lambda x: x.get("username", ""))
            return statuses


def format_duration(seconds: float) -> str:
    """Format seconds into human-readable duration."""
    if seconds < 60:
        return f"{int(seconds)}s"
    elif seconds < 3600:
        return f"{int(seconds // 60)}m {int(seconds % 60)}s"
    else:
        hours = int(seconds // 3600)
        minutes = int((seconds % 3600) // 60)
        return f"{hours}h {minutes}m"


def get_disk_free_space(path: str) -> tuple:
    """
    Get free disk space for a given path.
    Returns (free_gb, total_gb, percent_free)
    """
    try:
        if not path or path == "-":
            return None, None, None
        
        # Extract drive/root from path
        if os.name == 'nt':  # Windows
            # Handle paths like M:\folder\file.mp4 or M:/folder
            if len(path) >= 2 and path[1] == ':':
                drive = path[:3]  # e.g., "M:\\"
            else:
                drive = os.path.splitdrive(path)[0]
                if drive:
                    drive += os.sep
                else:
                    return None, None, None
        else:  # Unix
            drive = "/"
        
        if os.path.exists(drive):
            usage = shutil.disk_usage(drive)
            free_gb = usage.free / (1024 ** 3)
            total_gb = usage.total / (1024 ** 3)
            percent_free = (usage.free / usage.total) * 100
            return free_gb, total_gb, percent_free
    except Exception:
        pass
    return None, None, None


def get_recording_drives(statuses: list, check_path: str = None) -> dict:
    """
    Extract unique drives from recording paths.
    Returns dict of {drive: (free_gb, total_gb, percent_free)}
    """
    drives = {}
    
    # Always check the default/monitored path first
    if check_path:
        check_path = os.path.abspath(check_path)
        if os.name == 'nt':
            drive = os.path.splitdrive(check_path)[0]
            if drive:
                drive += os.sep
            else:
                drive = check_path
        else:
            drive = "/" # simplified for unix
            
        free_gb, total_gb, percent_free = get_disk_free_space(check_path)
        if free_gb is not None:
             drives[drive] = (free_gb, total_gb, percent_free)

    for status in statuses:
        current_file = status.get("current_file", "")
        if current_file and current_file != "-":
            if os.name == 'nt' and len(current_file) >= 2 and current_file[1] == ':':
                drive = current_file[:2].upper() + os.sep
            else:
                drive = "/"
            
            if drive not in drives:
                free_gb, total_gb, percent_free = get_disk_free_space(current_file)
                if free_gb is not None:
                    drives[drive] = (free_gb, total_gb, percent_free)
    return drives


def get_state_display(state: str, is_stale: bool) -> tuple:
    """
    Get display text and color for a state.
    Returns (display_text, color_code)
    """
    if is_stale:
        return ("⚠️  STALE", "yellow")
    
    state_map = {
        "STARTING": ("🔄 STARTING", "cyan"),
        "WAITING": ("⏳ WAITING", "blue"),
        "RECORDING": ("🔴 RECORDING", "red"),
        "STOPPED": ("⏹️  STOPPED", "grey"),
    }
    return state_map.get(state, (state, "white"))


def create_rich_table(statuses: list) -> Table:
    """Create a rich table from status data."""
    table = Table(
        title="TikTok Live Recorder - Instance Monitor",
        title_style="bold magenta",
        show_header=True,
        header_style="bold white on blue",
    )
    
    table.add_column("Username", style="cyan", width=16, no_wrap=True)
    table.add_column("Status", justify="center", width=14, no_wrap=True)
    table.add_column("Heartbeat", justify="center", width=12, no_wrap=True)
    table.add_column("PID", justify="right", width=8, no_wrap=True)
    table.add_column("Current File", width=40, no_wrap=True, overflow="ellipsis")
    table.add_column("Size (MB)", justify="right", width=10, no_wrap=True)
    
    if not statuses:
        table.add_row(
            "[dim]No instances running[/dim]",
            "", "", "", "", ""
        )
        return table
    
    for status in statuses:
        username = status.get("username", "?")
        state = status.get("state", "UNKNOWN")
        is_stale = status.get("is_stale", False)
        age_seconds = status.get("age_seconds", 0)
        pid = str(status.get("pid", "?"))
        current_file = status.get("current_file", "-")
        file_size = status.get("file_size_mb", 0)
        
        # Format state with color
        state_text, state_color = get_state_display(state, is_stale)
        
        # Format heartbeat age
        heartbeat_text = format_duration(age_seconds)
        heartbeat_style = "red" if is_stale else ("green" if age_seconds < 10 else "yellow")
        
        # Format file size
        size_text = f"{file_size:.1f}" if file_size > 0 else "-"
        
        # Truncate filename if too long
        if current_file and len(current_file) > 35:
            current_file = "..." + current_file[-32:]
        
        table.add_row(
            username,
            f"[{state_color}]{state_text}[/{state_color}]",
            f"[{heartbeat_style}]{heartbeat_text} ago[/{heartbeat_style}]",
            pid,
            current_file or "-",
            size_text
        )
    
    return table


def print_plain_table(statuses: list, check_path: str = None) -> None:
    """Print a plain text table (fallback without rich)."""
    # Clear screen
    os.system('cls' if os.name == 'nt' else 'clear')
    
    print("=" * 80)
    print("   TikTok Live Recorder - Instance Monitor")
    print("=" * 80)
    print()
    
    if not statuses:
        print("   No instances running.")
        print()
    
    # Header
    print(f"{'Username':<15} {'Status':<12} {'Heartbeat':<12} {'PID':<8} {'File':<25} {'Size MB':<8}")
    print("-" * 80)
    
    for status in statuses:
        username = status.get("username", "?")[:14]
        state = status.get("state", "UNKNOWN")
        is_stale = status.get("is_stale", False)
        age_seconds = status.get("age_seconds", 0)
        pid = str(status.get("pid", "?"))
        current_file = status.get("current_file", "-")
        file_size = status.get("file_size_mb", 0)
        
        # State indicator
        if is_stale:
            state_display = "!! STALE"
        elif state == "RECORDING":
            state_display = ">> REC"
        elif state == "WAITING":
            state_display = ".. WAIT"
        else:
            state_display = state[:10]
        
        # Truncate filename
        if current_file and len(current_file) > 24:
            current_file = "..." + current_file[-21:]
        
        size_text = f"{file_size:.1f}" if file_size > 0 else "-"
        heartbeat = format_duration(age_seconds)
        
        print(f"{username:<15} {state_display:<12} {heartbeat:<12} {pid:<8} {current_file or '-':<25} {size_text:<8}")
    
    # Show disk space info
    drives = get_recording_drives(statuses, check_path)
    if drives:
        print()
        print("   Disk Space:")
        for drive, (free_gb, total_gb, percent_free) in drives.items():
            bar_len = 20
            filled = int((100 - percent_free) / 100 * bar_len)
            bar = "#" * filled + "-" * (bar_len - filled)
            print(f"   {drive} [{bar}] {free_gb:.1f} GB free / {total_gb:.1f} GB ({percent_free:.0f}% free)")
    
    print()
    print(f"   Last refresh: {datetime.now().strftime('%H:%M:%S')}")
    print("   Press Ctrl+C to exit")


# Hide stale entries older than this (1.5 hours = 5400 seconds)
STALE_HIDE_THRESHOLD = 5400


def run_rich_dashboard(status_dir: str, refresh_interval: float, check_path: str = None) -> None:
    """Run the dashboard using rich library."""
    console = Console()
    
    def generate_display():
        all_statuses = get_all_statuses(status_dir)
        # Filter out very old stale entries (older than 1.5 hours)
        statuses = [s for s in all_statuses if s.get("age_seconds", 0) < STALE_HIDE_THRESHOLD]
        table = create_rich_table(statuses)
        
        # Get disk space info
        drives = get_recording_drives(statuses, check_path)
        
        footer = Text()
        footer.append(f"\nLast refresh: {datetime.now().strftime('%H:%M:%S')} ", style="dim")
        footer.append("| Press Ctrl+C to exit", style="dim")
        footer.append(f" | Status dir: {status_dir}", style="dim blue")
        
        # Add disk info with proper styling
        for drive, (free_gb, total_gb, percent_free) in drives.items():
            color = "green" if percent_free > 20 else ("yellow" if percent_free > 10 else "red")
            footer.append(" | ", style="dim")
            footer.append(f"{drive} {free_gb:.1f} GB free ({percent_free:.0f}%)", style=color)
        
        return Panel.fit(
            table,
            subtitle=footer,
            border_style="blue"
        )
    
    try:
        # Use higher refresh rate for smoother updates, screen=False to avoid full redraws
        with Live(generate_display(), refresh_per_second=4, console=console, screen=False, vertical_overflow="visible") as live:
            while True:
                time.sleep(refresh_interval)
                live.update(generate_display())
    except KeyboardInterrupt:
        console.print("\n[yellow]Monitor stopped.[/yellow]")


def run_plain_dashboard(status_dir: str, refresh_interval: float, check_path: str = None) -> None:
    """Run the dashboard using plain text output."""
    try:
        while True:
            all_statuses = get_all_statuses(status_dir)
            # Filter out very old stale entries (older than 1.5 hours)
            statuses = [s for s in all_statuses if s.get("age_seconds", 0) < STALE_HIDE_THRESHOLD]
            print_plain_table(statuses, check_path)
            time.sleep(refresh_interval)
    except KeyboardInterrupt:
        print("\n\nMonitor stopped.")


def main():
    parser = argparse.ArgumentParser(
        description="TikTok Live Recorder - Multi-Instance Monitor Dashboard",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
Examples:
    python monitor.py                      # Use default settings
    python monitor.py --refresh 1          # Faster refresh (1 second)
    python monitor.py --status-dir /path   # Custom status directory
        """
    )
    
    parser.add_argument(
        "--status-dir", "-d",
        default=DEFAULT_STATUS_DIR,
        help=f"Status directory (default: {DEFAULT_STATUS_DIR})"
    )
    
    parser.add_argument(
        "--refresh", "-r",
        type=float,
        default=2.0,
        help="Refresh interval in seconds (default: 2)"
    )
    
    parser.add_argument(
        "--path",
        default=None,
        help="Path to check for free disk space (default: auto-detect from recordings)"
    )
    
    parser.add_argument(
        "--plain", "-p",
        action="store_true",
        help="Force plain text mode (no rich formatting)"
    )
    
    args = parser.parse_args()
    
    # Print startup info
    print(f"Starting TikTok Live Recorder Monitor...")
    print(f"Status directory: {os.path.abspath(args.status_dir)}")
    print(f"Recording directory: {os.path.abspath(args.path) if args.path else '(auto-detect from recordings)'}")
    print(f"Refresh interval: {args.refresh}s")
    
    if not HAS_RICH:
        print("Note: Install 'rich' library for better visuals: pip install rich")
    
    print()
    time.sleep(1)  # Brief pause to show info
    
    # Run appropriate dashboard
    if args.plain or not HAS_RICH:
        run_plain_dashboard(args.status_dir, args.refresh, args.path)
    else:
        run_rich_dashboard(args.status_dir, args.refresh, args.path)


if __name__ == "__main__":
    main()
