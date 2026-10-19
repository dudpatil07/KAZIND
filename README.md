# KAZIND — Drone Threat Simulation Trainer

Unity project for a simulated drone identification game. The saved environment, scripts, materials, models, narration, tutorial videos, and project settings are included in this repository.

## Open on the college PC

1. Install **Unity Hub** and **Unity Editor 6000.0.84f1 (Unity 6 LTS)**. Use the version recorded in `ProjectSettings/ProjectVersion.txt`.
2. Clone the repository:
   ```sh
   git clone https://github.com/dudpatil07/KAZIND.git
   ```
   Alternatively, use GitHub **Code → Download ZIP**, then fully extract it.
3. In Unity Hub, select **Projects → Add → Add project from disk**. Select the `KAZIND` folder containing **Assets**, **Packages**, and **ProjectSettings**. For a ZIP download, this is usually `KAZIND-main`.
4. Open with the specified Editor version. Allow the first import and Unity Package Manager downloads to finish; internet access is needed. If the college PC restricts installations, ask its administrator to install Unity Hub/Editor and the relevant modules.
5. After the first import, Unity automatically opens the saved simulator scene. Press the toolbar **Play** button, or choose **KAZIND → Play Simulator**. Both launch **Assets/Scenes/SampleScene.unity**, even if another scene was open. The desktop version starts with the same tutorial video and menus as the original PC; **Esc** skips the video.

Open the project in Unity Hub, not the generated `.sln`/`.csproj` files or the Linux shell launcher. `Library`, `Temp`, IDE files, and build outputs are generated locally and are intentionally excluded from Git. No paid Asset Store download or Git LFS step is required for the included assets.

## Play on Meta Quest 3

See **[Quest 3 setup and controls](Docs/Quest3-Setup.md)** for both routes:

- **Windows PC + Quest Link:** install Meta Quest Link, set it as the active OpenXR runtime, and use **26247 → Quest 3 → Configure Windows Quest Link** in Unity. This route does not require Android Build Support.
- **Standalone headset app:** install Unity's Android Build Support with SDK/NDK Tools and OpenJDK, then use **26247 → Quest 3 → Configure Android VR** and build for Android.

The project includes a runtime headset rig, Touch controller input, menus, gallery, and HUD for VR. Desktop controls remain available when XR is inactive. **Quest hardware behaviour and performance still require testing on the college PC/headset; this repository is not a tested Quest release.**

## Desktop controls

| Action | Control |
| --- | --- |
| Move | W / A / S / D |
| Look | Mouse |
| Jump | Space |
| Run | Left Shift |
| Lock onto a centred drone | F, E, or left mouse click |
| Scope | Hold left mouse button |
| Classify when prompted | J: civilian; K: military |
| Release cursor / skip intro | Esc |
| Compare graphics modes | F8 |

## Troubleshooting

- **Empty/default scene on another PC:** close Unity, update this checkout with `git pull`, and reopen it. For ZIP downloads, extract a fresh download into a new folder and add that folder in Unity Hub. After compilation, choose **KAZIND → Play Simulator**. The saved environment is already included; no map rebuilding commands are needed. Unsaved scenes are preserved, and startup does not regenerate or resave the environment.
- **Missing packages or compiler errors:** let Package Manager finish, then inspect the first error in **Window → General → Console**. Verify internet access to Unity's package registry and the Editor version.
- **Quest Link installer fails under Wine:** official Quest Link requires Windows. Use a Windows PC for Link or build an Android APK from Linux.
- **Missing Android build target:** in Unity Hub, add Android Build Support, Android SDK & NDK Tools, and OpenJDK to this exact Editor version, then restart Unity.
- **Updating an existing checkout:** save your changes before `git pull`; resolve conflicts rather than overwriting work.

## Repository contents

- `Assets/` — saved scene, game scripts, models, materials, shaders, audio, and runtime videos, including Unity `.meta` files.
- `Packages/` — pinned Unity package manifest and dependency lockfile.
- `ProjectSettings/` — Unity version and project configuration.
- `Docs/` — setup and presentation notes.
- `Preview/` — recorded gameplay preview and its source recording.
- `TutorialVideo/` and `Tools/` — tutorial sources and optional media-generation utilities. These utilities are not needed to open or play the Unity project.
