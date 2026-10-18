"""Edit an actual gameplay recording into a 60-second narrated preview.

Usage: python3 Tools/make_gameplay_preview.py path/to/gameplay.mp4
Requires ffmpeg, ffprobe, numpy. No external visual footage is used.
"""
import argparse
import json
from pathlib import Path
import subprocess
import wave

import numpy as np

ROOT = Path(__file__).resolve().parents[1]
RATE = 48000
DURATION = 60
CUES = [
    (1.0, "welcome", "Welcome to the Drone Threat Simulator.\nStart by choosing a mission."),
    (15.0, "controls", "Move with W, A, S, and D,\nand aim with your mouse."),
    (42.0, "score", "Correct calls earn one hundred points."),
    (56.5, "good_luck", "Good luck."),
]


def run(*args):
    subprocess.run([str(arg) for arg in args], check=True)


def stamp(seconds):
    ms = round(seconds * 1000)
    return f"{ms // 3600000:02}:{ms // 60000 % 60:02}:{ms // 1000 % 60:02},{ms % 1000:03}"


def soundtrack(folder):
    """Original quiet electronic bed, ducked under the project's existing voice."""
    count = RATE * DURATION
    t = np.arange(count, dtype=np.float64) / RATE
    bed = np.zeros(count, dtype=np.float64)
    # A spacious A-minor progression. Deterministic noise is used for soft hats.
    chords = [(110, 130.81, 164.81), (87.31, 110, 130.81),
              (130.81, 164.81, 196), (98, 123.47, 146.83)]
    for start in range(0, DURATION, 8):
        stop = min(start + 8, DURATION)
        local = t[:round((stop - start) * RATE)]
        envelope = np.minimum(local / 1.0, 1) * np.minimum((stop - start - local) / 1.4, 1)
        chord = sum(np.sin(2 * np.pi * frequency * local) for frequency in chords[(start // 8) % 4])
        bed[start * RATE:stop * RATE] += .024 * chord * envelope
    rng = np.random.default_rng(27)
    for beat in np.arange(0, DURATION, 2 / 3):
        start = round(beat * RATE)
        length = min(round(.22 * RATE), count - start)
        local = np.arange(length) / RATE
        kick = np.sin(2 * np.pi * (48 * local + 35 * .025 * (1 - np.exp(-local / .025))))
        bed[start:start + length] += .095 * kick * np.exp(-local * 22)
        hat_start = start + RATE // 3
        hat_length = min(RATE // 20, count - hat_start)
        if hat_length > 0:
            noise = rng.uniform(-1, 1, hat_length)
            bed[hat_start:hat_start + hat_length] += .012 * noise * np.exp(-np.arange(hat_length) / (RATE * .012))
    speech = np.zeros(count, dtype=np.float64)
    duck = np.ones(count)
    captions = []
    for index, (start, name, caption) in enumerate(CUES, 1):
        source = ROOT / "Assets/Resources/Presentation/Voice" / f"{name}.wav"
        decoded = subprocess.check_output([
            "ffmpeg", "-v", "error", "-i", str(source), "-ar", str(RATE),
            "-ac", "1", "-f", "f32le", "-",
        ])
        samples = np.frombuffer(decoded, dtype="<f4").astype(np.float64)
        peak = max(float(np.max(np.abs(samples))), .001)
        samples *= .7 / peak
        offset = round(start * RATE)
        speech[offset:offset + len(samples)] += samples
        stop = start + len(samples) / RATE
        duck = np.minimum(duck, 1 - .7 * np.clip((t - start + .25) / .25, 0, 1)
                          * np.clip((stop + .4 - t) / .4, 0, 1))
        caption = caption.replace("\n", " ")
        captions.append(f"{index}\n{stamp(start)} --> {stamp(stop)}\n{caption}\n")
    fade = np.clip(t / 1.2, 0, 1) * np.clip((DURATION - t) / 1.8, 0, 1)
    mix = np.clip((speech + bed * duck) * fade, -.98, .98)
    with wave.open(str(folder / "soundtrack.wav"), "wb") as output:
        output.setnchannels(2)
        output.setsampwidth(2)
        output.setframerate(RATE)
        stereo = np.repeat((mix * 32767).astype("<i2")[:, None], 2, axis=1)
        output.writeframes(stereo.tobytes())
    (folder / "captions.srt").write_text("\n".join(captions))


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("recording", type=Path)
    parser.add_argument("--output", type=Path, default=ROOT / "Preview/DroneThreatSimulator-60s.mp4")
    args = parser.parse_args()
    recording = args.recording.resolve()
    probe = json.loads(subprocess.check_output([
        "ffprobe", "-v", "error", "-show_format", "-show_streams", "-of", "json", str(recording)
    ]))
    if float(probe["format"]["duration"]) < DURATION:
        raise SystemExit("Record at least 60 seconds of actual gameplay; footage is never looped or fabricated.")
    folder = args.output.resolve().parent
    folder.mkdir(parents=True, exist_ok=True)
    soundtrack(folder)
    # Relative subtitle filename avoids ffmpeg filter escaping for the project path.
    vf = (
        "fps=30,scale=1920:1080:force_original_aspect_ratio=decrease:force_divisible_by=2,"
        "pad=1920:1080:(ow-iw)/2:(oh-ih)/2,setsar=1,"
        "fade=t=in:d=0.4,fade=t=out:st=59.3:d=0.7,"
        "subtitles=captions.srt:force_style='FontName=DejaVu Sans,FontSize=10,"
        "Outline=0.6,Shadow=0.3,MarginV=2,Alignment=2'"
    )
    subprocess.run([
        "ffmpeg", "-hide_banner", "-loglevel", "warning", "-y",
        "-i", str(recording), "-i", "soundtrack.wav", "-t", "60",
        "-map", "0:v:0", "-map", "1:a:0", "-vf", vf,
        "-c:v", "libx264", "-preset", "medium", "-crf", "18", "-pix_fmt", "yuv420p",
        "-c:a", "aac", "-b:a", "192k", "-movflags", "+faststart",
        str(args.output.resolve()),
    ], cwd=folder, check=True)
    (folder / "provenance.json").write_text(json.dumps({
        "visual_source": str(recording), "duration_seconds": 60,
        "visual_edit": "Game-view-only capture, aspect-preserving fit, short opening/closing fades, captions",
        "voice_source": "Existing project narration; welcome, controls, score, good_luck",
        "music": "Original programmatically generated quiet electronic bed",
        "external_visual_footage": False,
    }, indent=2) + "\n")
    print(args.output.resolve())


if __name__ == "__main__":
    main()
