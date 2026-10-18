# Gameplay preview

`DroneThreatSimulator-60s.mp4` is the finished 60-second, 1920×1080, 30 fps preview.

The visuals are a continuous recording of your live Unity Game view, captured after your Ready reply. The editor panels were cropped out. The original game-view resolution is 1720×870, fitted into 1080p without stretching. No external footage or illustrated tutorial slides were used.

The edit adds short opening/closing fades, captions, four excerpts from the project’s existing narrator, and an original quiet electronic soundtrack. Desktop/system audio was not recorded.

`gameplay-original.mkv` preserves the unedited Game-view recording. `captions.srt`, `soundtrack.wav`, and `provenance.json` accompany the export.

To edit the same footage again, run from the project root:

```sh
python3 Tools/make_gameplay_preview.py Preview/gameplay-original.mkv
```

For a new recording, use `Tools/record_gameplay_preview.py --help`. Verify the current Unity window ID and Game-view rectangle first; the script preserves existing raw recordings.
