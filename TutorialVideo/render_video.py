from pathlib import Path
import subprocess
import sys

from PIL import Image, ImageDraw, ImageFont


ROOT = Path(__file__).resolve().parent
WIDTH, HEIGHT = 1280, 720
INK = (7, 15, 20)
LIME = (160, 255, 114)
CYAN = (91, 226, 255)
WHITE = (244, 248, 245)
MUTED = (183, 199, 197)
FONT_PATH = "/usr/share/fonts/truetype/noto/NotoSans-Regular.ttf"
BOLD_PATH = "/usr/share/fonts/truetype/noto/NotoSans-Bold.ttf"


def font(size, bold=False):
    path = BOLD_PATH if bold else FONT_PATH
    return ImageFont.truetype(path, size)


def background():
    art = Image.open(ROOT / "tutorial-hero.png").convert("RGB")
    scale = max(WIDTH / art.width, HEIGHT / art.height)
    art = art.resize((round(art.width * scale), round(art.height * scale)), Image.Resampling.LANCZOS)
    left = (art.width - WIDTH) // 2
    top = (art.height - HEIGHT) // 2
    art = art.crop((left, top, left + WIDTH, top + HEIGHT)).convert("RGBA")
    shade = Image.new("RGBA", (WIDTH, HEIGHT), (0, 0, 0, 0))
    sd = ImageDraw.Draw(shade)
    for x in range(WIDTH):
        alpha = int(210 * max(0, 1 - x / (WIDTH * 0.95)))
        sd.line((x, 0, x, HEIGHT), fill=(INK[0], INK[1], INK[2], alpha))
    art.alpha_composite(shade)
    return art


def rounded(draw, box, fill, outline=None, radius=14, width=2):
    draw.rounded_rectangle(box, radius=radius, fill=fill, outline=outline, width=width)


def base_scene(number, section, title, subtitle):
    image = background()
    draw = ImageDraw.Draw(image, "RGBA")
    draw.rectangle((0, 0, WIDTH, 5), fill=LIME + (255,))
    draw.text((60, 34), "DTS   /   FIELD GUIDE", font=font(17, True), fill=LIME + (255,))
    draw.text((WIDTH - 280, 34), f"TUTORIAL  {number} / 04", font=font(15, True), fill=WHITE + (220,))
    draw.text((62, 110), section.upper(), font=font(17, True), fill=CYAN + (255,))
    draw.text((60, 143), title, font=font(45, True), fill=WHITE + (255,), stroke_width=1, stroke_fill=(0, 0, 0, 110))
    draw.text((64, 204), subtitle, font=font(20), fill=MUTED + (255,))
    return image, draw


def make_frames():
    out = ROOT / "frames"
    out.mkdir(exist_ok=True)

    # Scene 1: clear mission selection cards
    image, d = base_scene("01", "Mission select", "Choose your mission", "Three levels. One clear objective: identify every target.")
    cards = [
        ("01", "CIVILIAN TRACKING", "3 civilian drones", LIME),
        ("02", "MIXED AIRSPACE", "4 drones · mixed class", CYAN),
        ("03", "FULL THREAT RESPONSE", "All 6 drones", (255, 191, 90)),
    ]
    y = 286
    for number, name, desc, accent in cards:
        rounded(d, (62, y, 600, y + 96), (7, 15, 20, 225), accent + (210,), 12, 2)
        d.text((84, y + 15), number, font=font(31, True), fill=accent + (255,))
        d.text((150, y + 13), name, font=font(18, True), fill=WHITE + (255,))
        d.text((150, y + 48), desc, font=font(16), fill=MUTED + (255,))
        d.text((527, y + 33), "START  ›", font=font(14, True), fill=accent + (255,))
        y += 108
    d.text((64, 563), "NEW HERE?  OPEN THE FIELD MANUAL FROM THE MENU.", font=font(15, True), fill=WHITE + (235,))
    image.convert("RGB").save(out / "01-missions.png", quality=95)

    # Scene 2: aim lock callout and time bar
    image, d = base_scene("02", "Acquire a target", "Hold the lock", "Aim at a drone. Press F, or use the Lock Aim button.")
    cx, cy = 906, 339
    for radius, color, width in [(66, LIME, 3), (49, CYAN, 1)]:
        d.ellipse((cx-radius, cy-radius, cx+radius, cy+radius), outline=color + (230,), width=width)
    d.line((cx-94, cy, cx-34, cy), fill=LIME + (255,), width=3)
    d.line((cx+34, cy, cx+94, cy), fill=LIME + (255,), width=3)
    d.line((cx, cy-94, cx, cy-34), fill=LIME + (255,), width=3)
    d.line((cx, cy+34, cx, cy+94), fill=LIME + (255,), width=3)
    rounded(d, (62, 303, 593, 478), (6, 15, 20, 232), (91, 226, 255, 190), 13, 2)
    d.text((88, 326), "AIM LOCK", font=font(17, True), fill=CYAN + (255,))
    d.text((88, 365), "10.0 s", font=font(48, True), fill=WHITE + (255,))
    d.text((88, 429), "Keep the target centered. If aim breaks, lock resets.", font=font(15), fill=MUTED + (255,))
    rounded(d, (88, 462, 565, 473), (60, 80, 82, 200), None, 5)
    rounded(d, (88, 462, 326, 473), LIME + (255,), None, 5)
    d.text((64, 533), "WASD MOVE   ·   MOUSE LOOK   ·   ESC SHOWS CURSOR", font=font(16, True), fill=WHITE + (255,))
    image.convert("RGB").save(out / "02-lock.png", quality=95)

    # Scene 3: class identification buttons and countdown
    image, d = base_scene("03", "Identify", "Make the call", "The aircraft is down. Identify its class within 10 seconds.")
    rounded(d, (66, 266, 711, 430), (5, 13, 18, 235), (160, 255, 114, 200), 15, 2)
    d.text((96, 286), "DOWNED AIRCRAFT", font=font(17, True), fill=LIME + (255,))
    d.text((96, 327), "AIRFRAME  /  inspect the shape", font=font(20, True), fill=WHITE + (255,))
    d.text((96, 364), "SIGNATURE  /  read the threat level", font=font(20, True), fill=WHITE + (255,))
    d.text((96, 402), "TIME REMAINING", font=font(14, True), fill=MUTED + (255,))
    d.text((590, 389), "08.4", font=font(34, True), fill=(255, 191, 90, 255))
    rounded(d, (66, 453, 370, 526), (13, 31, 36, 245), (91, 226, 255, 220), 12, 2)
    rounded(d, (392, 453, 696, 526), (26, 37, 20, 245), (160, 255, 114, 220), 12, 2)
    d.text((111, 476), "CIVILIAN  [ J ]", font=font(20, True), fill=WHITE + (255,))
    d.text((447, 476), "MILITARY  [ K ]", font=font(20, True), fill=WHITE + (255,))
    d.text((68, 546), "Correct +100   ·   Wrong −50   ·   Time-out: target escapes", font=font(15, True), fill=WHITE + (245,))
    image.convert("RGB").save(out / "03-identify.png", quality=95)

    # Scene 4: finish and controls recap
    image, d = base_scene("04", "Mission complete", "Clear the airspace", "Identify every mission target to finish the level.")
    rounded(d, (65, 287, 667, 490), (4, 13, 17, 235), (160, 255, 114, 205), 16, 2)
    d.text((98, 319), "MISSION COMPLETE", font=font(27, True), fill=LIME + (255,))
    d.text((98, 371), "TARGETS IDENTIFIED     6 / 6", font=font(18, True), fill=WHITE + (255,))
    d.text((98, 415), "FINAL SCORE     600", font=font(20, True), fill=CYAN + (255,))
    rounded(d, (99, 445, 343, 481), (38, 79, 47, 255), None, 8)
    d.text((126, 452), "REPLAY MISSION", font=font(15, True), fill=WHITE + (255,))
    d.text((65, 525), "QUICK CONTROLS", font=font(17, True), fill=CYAN + (255,))
    d.text((65, 558), "WASD  move     F  lock     J  civilian     K  military", font=font(17, True), fill=WHITE + (255,))
    image.convert("RGB").save(out / "04-complete.png", quality=95)


def render_video():
    make_frames()
    frame_dir = ROOT / "frames"
    frames = [frame_dir / f for f in ("01-missions.png", "02-lock.png", "03-identify.png", "04-complete.png")]
    lengths = [16.5, 18.5, 15.5, 14.5]
    fade = 0.7
    command = ["ffmpeg", "-y", "-hide_banner", "-loglevel", "warning"]
    for frame, length in zip(frames, lengths):
        command += ["-loop", "1", "-t", str(length), "-i", str(frame)]
    command += ["-i", str(ROOT / "narration.mp3")]

    filters = []
    for i in range(len(frames)):
        filters.append(f"[{i}:v]scale={WIDTH}:{HEIGHT},setsar=1,fps=30,zoompan=z='min(zoom+0.00006,1.04)':d=1:s={WIDTH}x{HEIGHT}:fps=30,format=yuv420p,settb=AVTB,setpts=PTS-STARTPTS[v{i}]")
    last = "v0"
    accumulated = lengths[0]
    for i in range(1, len(frames)):
        nxt = f"x{i}"
        offset = accumulated - fade
        filters.append(f"[{last}][v{i}]xfade=transition=fade:duration={fade}:offset={offset:.3f}[{nxt}]")
        last = nxt
        accumulated += lengths[i] - fade

    srt_path = (ROOT / "narration.srt").as_posix().replace(":", r"\:").replace("'", r"\'")
    filters.append(f"[{last}]drawbox=x=0:y=580:w=iw:h=140:color=black@0.38:t=fill,subtitles='{srt_path}':force_style='FontName=Noto Sans,FontSize=18,Outline=2,Shadow=1,MarginV=20,Alignment=2'[vout]")
    filters.append(f"[{len(frames)}:a]aresample=48000,apad,atrim=duration={accumulated:.3f},afade=t=out:st={accumulated-1.2:.3f}:d=1.2[aout]")

    command += [
        "-filter_complex", ";".join(filters),
        "-map", "[vout]", "-map", "[aout]",
        "-c:v", "libx264", "-preset", "medium", "-crf", "20",
        "-c:a", "aac", "-b:a", "160k", "-movflags", "+faststart",
        str(ROOT / "DroneThreatSimulator-HowToPlay.mp4"),
    ]
    subprocess.run(command, check=True)


if __name__ == "__main__":
    if len(sys.argv) > 1 and sys.argv[1] == "frames":
        make_frames()
    else:
        render_video()
