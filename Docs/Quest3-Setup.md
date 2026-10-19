# Quest 3 setup on the college PC

First open this repository in **Unity 6000.0.84f1** and verify that `Assets/Scenes/SampleScene.unity` runs on the desktop. Allow packages to resolve before looking for the `26247` menu.

## Option A: Play through Quest Link on Windows

Use a Windows PC supported by Meta Quest Link, with a compatible graphics card and a USB data cable. Official Quest Link is not supported on Linux/Wine. The headset streams the game from the PC with this route.

1. Install and open Meta Quest Link on Windows. Its installer needs free space on the Windows system drive for temporary files, even when installing to another drive.
2. In the Link application's settings, set **Meta Quest Link as the active OpenXR runtime** (normally under General).
3. Connect the Quest 3 by USB and complete the application's device setup. Put on the headset, open Quest Link, select the PC, and launch the cable connection. A file-transfer prompt alone does not start Link.
4. In Unity, select **26247 → Quest 3 → Configure Windows Quest Link**. This creates the Standalone OpenXR loader/settings and enables the Oculus Touch controller profile. Running the command again reuses those settings.
5. In **File → Build Profiles**, use **Windows**. Under **Edit → Project Settings → XR Plug-in Management**, confirm **OpenXR** and **Initialize XR on Startup** are enabled for Standalone. Check **Project Validation** and resolve errors.
6. Open `Assets/Scenes/SampleScene.unity`. With Quest Link already running in the headset, press **Play** in Unity.

Android Build Support and headset Developer Mode are not required for this PC VR workflow. The PC must have the project; installing Link alone does not install the simulator. Unity's monitor view may differ from the stereo headset view.

## Option B: Install a standalone APK over USB

This runs the game on the headset and works without the Link desktop app.

1. In Unity Hub, add **Android Build Support**, **Android SDK & NDK Tools**, and **OpenJDK** to Editor **6000.0.84f1**. Restart Unity after adding modules.
2. Enable headset **Developer Mode** using your Meta developer account/device setup. Connect by USB and accept the **USB debugging** authorization inside the headset.
3. In Unity, select **26247 → Quest 3 → Configure Android VR**. This sets OpenXR, Meta Quest support, the Oculus Touch controller profile, IL2CPP, ARM64, Vulkan, and Android API 32 as the minimum.
4. In **File → Build Profiles**, switch to **Android**. Include `Assets/Scenes/SampleScene.unity` and check **XR Plug-in Management → Project Validation** for errors.
5. Select the connected headset as the run device and use **Build And Run**. Alternatively, **26247 → Quest 3 → Build APK** writes `Builds/Quest/DroneThreatSimulator.apk` after Android is the active target. An APK built with that menu still needs to be installed onto the headset.
6. For an existing APK, the Android SDK's `adb install -r Builds/Quest/DroneThreatSimulator.apk` installs/updates it. The headset must appear as an authorized device in `adb devices` first. Launch the sideloaded app from the headset's developer/Unknown Sources library.

## Controller mapping

| Action | Quest control |
| --- | --- |
| Look / aim | Move your head; centre the drone on the reticle |
| Walk | Left thumbstick |
| Snap turn | Right thumbstick left/right; 30 degrees per push |
| Choose a level / browse gallery | Left thumbstick; release between selections |
| Open selected level / start / replay | A |
| Controls / back to levels | B in menus |
| Start the existing aim-lock sequence | Right trigger |
| Classify when prompted | A: civilian; B: military |
| Return to level selection, resetting the round | Y |

The headset rig attaches at runtime only when XR becomes active. VR uses world-space UI in both eyes, a head-directed reticle, and the Performance graphics preset. The keyboard tutorial video is skipped in VR; choose **B** on the level menu for Quest controls. Desktop FOV zoom and jumping are not mapped in VR.

## Validation and limits

The transfer was checked with the project's Unity 6 Editor for compilation, scene script references, repeated Quest Link configuration, and creation of the VR interface. The code is included for continuation on the college PC. A Windows Quest Link session, Android APK build, headset rendering/controller behaviour, and standalone performance still need testing on actual hardware. No ready-to-install APK is included.

If the headset shows Link Home while Unity runs, verify the active OpenXR runtime, Standalone loader, startup setting, and Unity Console, then restart Play mode after connecting Link.

Official references:

- [Meta: Use Link for app development](https://developers.meta.com/vr/documentation/unity/unity-link/)
- [Meta: Manual Unity project setup](https://developers.meta.com/vr/documentation/unity/unity-tutorial-hello-world-manual-steps/)
- [Meta: Use ADB with Meta Quest](https://developers.meta.com/vr/documentation/native/android/ts-adb/)
