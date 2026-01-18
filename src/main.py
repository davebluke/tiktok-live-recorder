import argparse
import sys
import os

# Ensure the script directory is in sys.path so we can import local modules
current_dir = os.path.dirname(os.path.abspath(__file__))
if current_dir not in sys.path:
    sys.path.insert(0, current_dir)

try:
    # Try importing directly first (if running from src)
    from tiktok import TikTok
except ImportError:
    try:
        # Try importing from src package (if running from root)
        from src.tiktok import TikTok
    except ImportError as e:
        print("[!] Error: Could not import 'TikTok' module.")
        print("    If you are missing dependencies, run: pip install -r requirements.txt")
        print(f"    Native Error: {e}")
        sys.exit(1)

def main():
    parser = argparse.ArgumentParser(description="TikTok Live Recorder")
    
    parser.add_argument("-user", required=True, help="TikTok Username")
    parser.add_argument("-mode", default="manual", help="manual or automatic")
    parser.add_argument("-output", default="./downloads", help="Output directory")
    parser.add_argument("-ffmpeg", default="ffmpeg", help="Path to ffmpeg (optional)")
    
    args = parser.parse_args()

    # Create the bot instance
    # The 'run' method in src/tiktok.py already handles the smart recording logic
    bot = TikTok(
        output=args.output,
        mode=args.mode,
        user=args.user,
        ffmpeg=args.ffmpeg
    )
    
    print(f"[*] Starting TikTok Recorder for {args.user} in {args.mode} mode...")
    bot.run()

if __name__ == "__main__":
    main()