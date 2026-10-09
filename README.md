KAZIND — Drone Threat Simulation Trainer
A Unity-based training simulation for identifying and responding to simulated drone threats.
Requirements
- Unity Hub
- Recommended Unity Editor version: Unity 6 (6000.0.84f1), if available
- Git (optional, if cloning the repository)
Open the project in Unity
Option 1: Clone from GitHub
1. Install and open Unity Hub.
2. Open a terminal and run:
   git clone https://github.com/dudpatil07/KAZIND.git
3. In Unity Hub, select Projects.
4. Click Add or Add project from disk.
5. Select the cloned KAZIND folder — the folder containing Assets, Packages, and ProjectSettings.
6. If prompted to choose an Editor version, select Unity 6 (6000.0.84f1) if installed. Otherwise, install that version through Unity Hub or choose a compatible Unity 6 version.
7. Open the project and wait for Unity to import assets and compile scripts.
Option 2: Open a downloaded project folder
1. Download or extract the project files.
2. Open Unity Hub and go to Projects.
3. Click Add or Add project from disk.
4. Choose the project root folder containing Assets, Packages, and ProjectSettings.
5. Open it and wait for the initial import and script compilation to finish.
Run the simulation
1. In Unity, open the scene used by the project. The default scene may be SampleScene.
2. Click Play to start the simulation.
3. Check Window → General → Console if errors appear during compilation or Play mode.
If the training environment or gameplay objects are missing, check the Unity Editor's 26247 menu for available setup/build commands. Run setup commands only when needed, verify the result, and save the scene.
Intended controls
Controls may vary depending on the current scene and scripts.
- W / A / S / D — Move
- Mouse — Look around
- Space — Jump
- Left Shift — Run
- Left mouse button or O — Scope/zoom
- E — Scan a detected drone when prompted
- J / K — Choose a threat classification when prompted
- Esc — Release the mouse cursor
Troubleshooting
- Unity asks to upgrade the project: Prefer the specified Unity version. Back up or commit your work before opening the project with a different Editor version.
- Scripts fail to compile: Open Window → General → Console, address the first reported error, and wait for Unity to recompile.
- Scene looks empty: Confirm that you opened the correct project root and scene. Check the 26247 menu for available environment-building commands.
- Assets are missing: Wait for Unity to finish importing. Do not delete Assets, Packages, or ProjectSettings.
- Git reports a conflict: Keep a backup and resolve the conflict instead of force-pushing.
Project structure
- Assets/ — Scenes, scripts, materials, models, and other Unity assets
- Packages/ — Unity package manifest and package configuration
- ProjectSettings/ — Unity project settings
Notes
- This project is intended as a simulated training environment.
- Avoid committing Unity-generated folders such as Library/, Temp/, Logs/, or build output unless specifically required.
- After making changes, save the scene and commit the relevant project files to Git.
