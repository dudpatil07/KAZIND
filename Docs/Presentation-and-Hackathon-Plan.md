# Presentation update and hackathon coverage

## What this change implements

- A scaled 1280 × 720 level menu with three scenario cards and visual-quality controls.
- A pre-level gallery reached from both level selection and replay. It displays a rotating 3D model, name and generic airframe description for a quadcopter, fixed-wing aircraft and five-drone formation.
- Procedural decorative models with contrasting shells, propellers, landing legs, camera housings, wings and tail surfaces. These are illustrative meshes, not replicas of operational aircraft. `DroneVisualModel` now also replaces the visible in-game meshes: scout/racer quadcopters, utility hexacopters and generic fixed-wing aircraft. Existing gameplay colliders are preserved. Static geometry is combined by material; only the propellers animate separately.
- A studio shader independent of world fog and lighting, stored under `Resources` so it is included in builds. The preview uses a single 768 × 512 render texture and only renders while the gallery is open.
- Original, Performance and Enhanced presentation. Enhanced changes daylight lighting, sky colors and ambient fill as well as ACES tone mapping, SMAA and a light vignette. Bloom and screen-space lens flare are explicitly overridden to zero to prevent bright sources washing out the scene. This suppression remains in Original mode; disabling `ScenePresentation` restores the original camera settings. Enhanced limits local shadow casters to two spotlights to reduce atlas pressure. Performance uses the daylight presentation with no local-light shadows or camera post-processing. Weather is not changed.
- Persisted drone slot assignments and runtime assignment on every level start, including older scenes that never saved slots. This fixes the shared-destination clustering bug. Rotor animation no longer rotates the structural support arms.
- Offline voice guidance with subtitles: welcome/menu navigation, movement controls, encouragement and score feedback. **H** replays help; **V** toggles voice. The footer also provides clickable controls after releasing the mouse. Muting preserves subtitles. Spoken cues are queued without overlapping, old cues are cleared on screen changes, and idle control reminders are limited to once every 90 seconds after the first minute. Prompts use the existing tutorial narrator and do not select targets or prescribe tactics.

The existing introductory video remains first in the flow. The gallery follows level selection; it is not an asynchronous scene loader and does not display invented loading progress. No external assets or packages are required beyond the project's existing URP and Input System packages.

## Try it in Unity

1. Open `Assets/Scenes/SampleScene.unity` in Unity 6000.0.84f1 and let scripts and the shader import.
2. Enter Play mode. Watch the existing intro or press Escape to skip it.
3. Select a presentation mode in the level menu. Start with Enhanced; compare Performance on the intended laptop. During gameplay, press **F8** to cycle Original → Performance → Enhanced. The HUD displays the active mode.
4. Choose **Explore Level**. Switch among all three tabs and check the model, name and description. The models are generic gallery examples; the text explicitly distinguishes them from the current level roster.
5. Select **Start Level** to enter the existing level. **Back to Levels** or Escape cancels the gallery. Replay also passes through the gallery.
6. For custom grading, edit the values in `ScenePresentation.Initialize`. Lighting is managed by `SceneLightingPresentation`. Changes are runtime-only. Original restores the prior lighting and grading while retaining glare suppression; disabling the component restores the original camera settings too. Stop and restart Play mode after importing this update so the manager installs the in-game visual replacements.

The white-glare fix removes bloom and screen-space flare rather than simply disabling their Volume components: an inactive component would allow another scene Volume to contribute its old bloom values. The status header now has a dark backing so it stays readable against the sky. Rendered checks of a copy of the actual scene showed no large white wash in the tested sun-facing and ground-facing views. The exact gameplay camera position in the supplied screenshot was not reproduced, so an additional scene-specific cause cannot be ruled out.

The four voice WAVs are bundled in `Assets/Resources/Presentation/Voice` and need no runtime internet connection or speech service. Their source is the project's existing `TutorialVideo/narration.mp3`. To regenerate the clips, run `python3 Tools/build_voice_prompts.py` with ffmpeg installed. Replace these WAVs and update the matching caption strings in `VoiceGuidance.cs` to change narration; captions should match the recording.

`DroneThreatSystem` attaches the presentation components automatically. Do not add another copy to a separate object. The preview's renderers use layer 31 in a remote studio at `(0, -10000, 0)`. Keep that location free of scene content. Its camera has a 15-unit far plane and does not need an AudioListener or an EventSystem. The components release their render texture, runtime materials and Volume profile when destroyed.

## Movement finding

The clustering bug came from `surroundSlot`: `SetupDroneAI` assigned distinct values in the Editor, but the private field was not serialized. Loading the scene reset all six values to zero. Every drone then used the same destination in `SurroundPlayer`. The field is now serialized, and `SelectLevel` reassigns unique slots every round so existing saved scenes also work. The patrol phase is persisted and staggered, and resetting an inactive drone captures its real spawn before `Awake` has run.

There is no direct aim-to-freeze branch in the current `TrainingDrone.Update` code. Two existing behaviors explain the apparent stopping:

- `SurroundPlayer` moves toward an almost stationary slot around the player; its only variation is a slow, small angular oscillation.
- After the lock timer finishes, `DroneThreatSystem.DownTarget` invokes `TrainingDrone.DownForIdentification`. That method deliberately sets `enabled = false` and teleports the drone down to identification height.

The existing hover behavior and identification pause are unchanged. The slot fix removes the shared-destination bug; it is not collision avoidance, so independent paths may still cross. Realistic aim-reactive evasion, military counter-drone procedures, jamming weapons and operational radar simulation are outside this presentation change. It makes no claims about SAKSHAM, laser systems or real-world equipment performance.

## Requirement audit

| Requirement | State in the inspected project after this update |
| --- | --- |
| Video introduction and level selection | Present; menu refreshed |
| Pre-level 3D profile display | Added with three generic illustrative designs |
| Three corresponding playable drone classes | Gameplay still has Cheap/Military classes and six variants; new quadcopter, hexacopter and fixed-wing visuals are installed in-game |
| Continuous evasive movement / boids | Not implemented in this change |
| Coordinated swarm simulation | Not present; gallery formation is decorative |
| Electronic-warfare response | Not implemented |
| Early-warning radar / degraded sensor contacts | Not present |
| Weather | Clear, fog and rain code exists; full scenario integration needs Play-mode review |
| Day/night and urban/rural presets | No complete scenario selector found |
| Procedural scenario generation | Not found; procedural environment-building tools are not randomized scenario generation |
| Decision-tree scoring | Not present; current code awards +100 for a correct identification and subtracts 50 for an incorrect attempt |
| AAR dashboard | Not present; only a level-complete score panel exists |
| Minimal-hardware performance | Performance presentation mode added; whole-game profiling is still needed |

## Follow-up plan for a non-combat educational demo

1. **Scenario data:** define a ScriptableObject containing an ID, deterministic seed, environment preset, lighting preset and duration. Save the seed with each session so a demonstration can be repeated.
2. **Environment variation:** assemble urban/rural scenery from reusable chunks with fixed walkable boundaries. Vary decorative placement using the scenario seed, and pool repeated objects. Add day/night lighting presets without changing gameplay rules.
3. **Synthetic sensor exercise:** make a separate UI demonstration using generated contacts, seeded dropouts and false positives. Label it synthetic and keep it independent of real sensor performance or military tactics.
4. **Educational decision tree:** define nodes for observe, describe, submit or request more information. Record the selected node, timestamp and rubric version. Score generic observation and interface tasks using explicit criteria, with distinct records for retries and timeouts.
5. **Session review/AAR:** build an event timeline, completed-task counts, retry counts and a comparison of expected versus selected answers for the educational exercise. Add JSON/CSV export with scenario ID and seed; review should explain the rubric rather than prescribe tactics.
6. **Performance and visual pass:** profile CPU time, GPU time and allocations on the actual minimum-spec machine. Reuse materials, add LODs to scenery, bake static lighting and reflections, and cap shadow distance and weather particles through quality presets. Replace gallery primitives with licensed art when higher detail is needed.

## Validation

The project's 16 runtime scripts compile with the installed Unity 6000.0.84f1 compiler and 299 assembly references. Compilation reports one pre-existing warning in `DroneExplosionFX.cs`: `ExplosionAnimator.light` hides an inherited member. All four voice files have valid, non-silent mono PCM samples at 24 kHz.

`Assets/Editor/ValidateDronePresentation.cs` provides batch-mode regression checks for slot serialization, reset before Awake, runtime round assignment, model replacement without duplicate geometry or extra colliders, volume visibility, and graphics-setting restoration. Run it in a separate project copy using `-batchmode -nographics -executeMethod ValidateDronePresentation.Run` without `-quit`. It creates an unsaved test scene and exits with a pass/fail code; it does not save the user's scene.

The checks also inspect the resolved Volume stack for zero bloom/flare intensity and exercise narration import, muted subtitles, help replay, queue clearing and context transitions.

These checks passed in Unity 6000.0.84f1 in an isolated temporary project (exit code 0). They imported the new assets and exercised Play mode with six aircraft. The log is `/tmp/drone-isolated-validation.log`. The glare test includes a legacy Volume with bloom intensity 20 and flare intensity 1, then verifies that the resolved stack suppresses both.

A separate OpenGL render run in a virtual display also completed successfully using a copy of the actual scene. Captures are `/tmp/drone-sun-before.png`, `/tmp/drone-sun-after.png` and `/tmp/drone-world-after.png`; the log is `/tmp/drone-render-validation.log`. These are camera render-texture captures, so they do not validate the GUI overlay. Audio data/imports were verified, but audible playback on the user's device, full UI navigation and frame rate still need Editor review:

- Intro → menu → each gallery tab → level, and gallery → Escape → menu.
- Level completion → replay → gallery → level.
- 1280 × 720, 1920 × 1080 and a narrow Game view: labels and buttons remain inside the scaled canvas.
- Each visual mode: Enhanced changes daylight, ambient fill and grading; Performance has no camera post-processing or local shadows; Original restores camera and lighting settings.
- All six aircraft have separate slots in the full level, both on first entry and replay. Paths may still cross; there is no collision-avoidance simulation.
- In-game airframes have the new meshes; propellers rotate while arms remain fixed. The old control-label overlay no longer covers the score panel.
- Look toward bright sky and lights in Enhanced and Original: there should be no bloom halo covering the scene. Check that the subtitle footer does not obscure menu buttons.
- Listen to level-start narration, press H during a sentence, then mute with V. Only one voice should play, while subtitles remain readable. Return to the menu and confirm the old level prompts stop.
- A standalone build: the gallery shader and introductory video load correctly.
- Open and close the gallery repeatedly; the preview camera is inactive during gameplay and model/material counts do not grow.

The existing scene and pre-existing uncommitted work were not reverted. The manager installs the presentation changes and assigns slots at runtime, so older scenes do not need rebuilding.
