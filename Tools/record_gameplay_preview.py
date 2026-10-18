"""Record only a supplied Unity Game-view rectangle, then make the preview.

Example (verify the current window ID and crop first):
python3 Tools/record_gameplay_preview.py --window 0x1200210 --crop 1720:870:100:110
The Unity window must stay visible, at the same size, for the entire recording.
No desktop audio is captured; the editor adds the project's existing narration.
"""
import argparse
import os
from pathlib import Path
import re
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[1]


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--window", required=True, help="X11 window ID of Unity, e.g. 0x1200210")
    parser.add_argument("--crop", required=True, help="Game-view width:height:left:top within Unity")
    parser.add_argument("--display", default=os.environ.get("DISPLAY", ":1"))
    args = parser.parse_args()
    int(args.window, 0)
    if not re.fullmatch(r"[1-9]\d*:[1-9]\d*:\d+:\d+", args.crop):
        parser.error("--crop must be positive width:height and nonnegative left:top")
    folder = ROOT / "Preview"
    folder.mkdir(exist_ok=True)
    recording = folder / "gameplay-original.mkv"
    if recording.exists():
        parser.error(f"Preserving existing recording: {recording}. Move it before recording again.")
    print("Recording your Unity Game view for 60 seconds. Keep the window visible.", flush=True)
    subprocess.run([
        "ffmpeg", "-hide_banner", "-loglevel", "warning", "-nostdin",
        "-thread_queue_size", "512", "-f", "x11grab", "-draw_mouse", "0",
        "-framerate", "30", "-window_id", args.window, "-i", args.display,
        "-t", "60", "-vf", f"crop={args.crop},pad=ceil(iw/2)*2:ceil(ih/2)*2",
        "-an", "-c:v", "libx264", "-preset", "ultrafast", "-crf", "18",
        "-pix_fmt", "yuv420p", str(recording),
    ], check=True)
    print("Recording complete. Adding voice, captions and soundtrack.", flush=True)
    subprocess.run([sys.executable, str(ROOT / "Tools/make_gameplay_preview.py"), str(recording)], check=True)


if __name__ == "__main__":
    main()
