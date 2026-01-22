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
    
    table.add_column("Username", style="cyan", min_width=15)
    table.add_column("Status", justify="center", min_width=12)
    table.add_column("Heartbeat", justify="center", min_width=10)
    table.add_column("PID", justify="right", min_width=8)
    table.add_column("Current File", min_width=30)
    table.add_column("Size (MB)", justify="right", min_width=10)
    
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


def print_plain_table(statuses: list) -> None:
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
        print("   Waiting for recorder instances to start...")
        print("   (run 'python main.py -user <username>' to start recording)")
        return
    
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
    
    print()
    print(f"   Last refresh: {datetime.now().strftime('%H:%M:%S')}")
    print("   Press Ctrl+C to exit")


def run_rich_dashboard(status_dir: str, refresh_interval: float) -> None:
    """Run the dashboard using rich library."""
    console = Console()
    
    def generate_display():
        statuses = get_all_statuses(status_dir)
        table = create_rich_table(statuses)
        
        footer = Text()
        footer.append(f"\nLast refresh: {datetime.now().strftime('%H:%M:%S')} ", style="dim")
        footer.append("| Press Ctrl+C to exit", style="dim")
        footer.append(f" | Status dir: {status_dir}", style="dim blue")
        
        return Panel.fit(
            table,
            subtitle=footer,
            border_style="blue"
        )
    
    try:
        with Live(generate_display(), refresh_per_second=1/refresh_interval, console=console) as live:
            while True:
                time.sleep(refresh_interval)
                live.update(generate_display())
    except KeyboardInterrupt:
        console.print("\n[yellow]Monitor stopped.[/yellow]")


def run_plain_dashboard(status_dir: str, refresh_interval: float) -> None:
    """Run the dashboard using plain text output."""
    try:
        while True:
            statuses = get_all_statuses(status_dir)
            print_plain_table(statuses)
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
        "--plain", "-p",
        action="store_true",
        help="Force plain text output (no rich formatting)"
    )
    
    args = parser.parse_args()
    
    # Print startup info
    print(f"Starting TikTok Live Recorder Monitor...")
    print(f"Status directory: {os.path.abspath(args.status_dir)}")
    print(f"Refresh interval: {args.refresh}s")
    
    if not HAS_RICH:
        print("Note: Install 'rich' library for better visuals: pip install rich")
    
    print()
    time.sleep(1)  # Brief pause to show info
    
    # Run appropriate dashboard
    if args.plain or not HAS_RICH:
        run_plain_dashboard(args.status_dir, args.refresh)
    else:
        run_rich_dashboard(args.status_dir, args.refresh)


if __name__ == "__main__":
    main()
