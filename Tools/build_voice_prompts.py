"""Extract general UI narration from the existing tutorial; requires local ffmpeg."""
from pathlib import Path
import subprocess

ROOT = Path(__file__).resolve().parents[1]
OUTPUT = ROOT / "Assets/Resources/Presentation/Voice"
CLIPS = (
    ("welcome", 0.100, 5.490),
    ("controls", 16.342, 6.026),
    ("score", 48.855, 3.184),
    ("good_luck", 61.723, 1.605),
)


def main():
    OUTPUT.mkdir(parents=True, exist_ok=True)
    for name, start, duration in CLIPS:
        subprocess.run([
            "ffmpeg", "-hide_banner", "-loglevel", "error", "-y",
            "-ss", str(start), "-t", str(duration),
            "-i", str(ROOT / "TutorialVideo/narration.mp3"),
            "-af", f"afade=t=in:d=0.015,afade=t=out:st={duration - 0.035}:d=0.035,alimiter=limit=0.9",
            "-ar", "24000", "-ac", "1", "-c:a", "pcm_s16le",
            str(OUTPUT / f"{name}.wav"),
        ], check=True)


if __name__ == "__main__":
    main()
