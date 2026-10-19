using UnityEngine;

public partial class DroneThreatSystem
{
    private QuestHud questHud;
    private int questMenuLevel;

    // Return true for states handled entirely by the VR interface.
    private bool UpdateQuestInput()
    {
        if (questHud == null)
        {
            questHud = QuestHud.Create(playerCamera);
            scenePresentation.SetMode(ScenePresentation.VisualMode.Performance);
            GameObject oldHud = GameObject.Find("TrainingHUD");
            if (oldHud != null) oldHud.SetActive(false);
            // The existing video teaches keyboard controls; VR has its own instructions.
            if (state == GameState.IntroVideo) FinishIntroVideo();
        }

        QuestPlayerRig input = QuestPlayerRig.Instance;
        if (!Application.isFocused || !input.HeadTracked) return true;
        if (input.YPressed)
        {
            ReturnToMenu();
            return true;
        }
        voiceGuidance.AllowReminders = false; // Existing reminders describe keyboard controls.
        if (state == GameState.MainMenu)
        {
            questMenuLevel = (questMenuLevel + input.MenuStep + LevelNames.Length) % LevelNames.Length;
            if (input.APressed) OpenBriefing(questMenuLevel);
            else if (input.BPressed) state = GameState.Tutorial;
            return true;
        }
        if (state == GameState.Tutorial)
        {
            if (input.APressed || input.BPressed) ReturnToMenu();
            return true;
        }
        if (state == GameState.Briefing)
        {
            if (input.MenuStep != 0) briefingScreen.CycleModel(input.MenuStep);
            if (input.APressed) briefingScreen.ContinueFromVR();
            else if (input.BPressed) ReturnToMenu();
            return true;
        }
        if (state == GameState.LevelComplete)
        {
            if (input.APressed) OpenBriefing(selectedLevel);
            else if (input.BPressed) ReturnToMenu();
            return true;
        }
        return false;
    }

    private void LateUpdate()
    {
        if (!QuestPlayerRig.Active || questHud == null) return;
        // Avoid desktop control reminders after the main gameplay update.
        voiceGuidance.AllowReminders = false;
        string title;
        string content;
        string controls;
        bool gameplay = false;
        Texture gallery = null;
        switch (state)
        {
            case GameState.MainMenu:
                title = "DRONE THREAT SIMULATOR";
                content = "Select a level\n\n";
                for (int i = 0; i < LevelNames.Length; i++)
                    content += (i == questMenuLevel ? "<color=#80FFC7>▶  " : "    ") + LevelNames[i] +
                        (i == questMenuLevel ? "</color>" : "") + "\n\n";
                controls = "LEFT STICK: select     A: open level     B: controls";
                break;
            case GameState.Tutorial:
                title = "QUEST CONTROLS";
                content = "Look around with your head.\nLeft stick: walk     Right stick: 30° snap turn\n\n" +
                    "Look at a drone and press RIGHT TRIGGER.\nKeep it centred while the lock timer counts down.\n\n" +
                    "When prompted: A = civilian, B = military.\nY returns to level selection and resets the round.";
                controls = "A / B: back to levels";
                break;
            case GameState.Briefing:
                title = "AIRFRAME GALLERY";
                content = briefingScreen.SelectedName + "\n\n" + briefingScreen.SelectedProfile;
                gallery = briefingScreen.PreviewTexture;
                controls = "LEFT STICK: browse     A: start level     B: back";
                break;
            case GameState.LevelComplete:
                title = "LEVEL COMPLETE";
                content = LevelNames[Mathf.Clamp(selectedLevel, 0, LevelNames.Length - 1)] + "\n\nScore: " + score;
                controls = "A: replay     B: level selection";
                break;
            default:
                gameplay = true;
                title = state == GameState.Locking ? "HOLD YOUR GAZE  " + Mathf.Max(0, detectionTime - lockTimer).ToString("0.0") + " s" :
                    state == GameState.Identifying ? "IDENTIFY  " + Mathf.Max(0, identifyTimer).ToString("0.0") + " s" : "LOOK AT A DRONE";
                content = "Score: " + score + "     Classified: " + identifiedCount + "/" + GetLevelDroneCount(selectedLevel);
                if (state == GameState.Identifying && currentTarget != null)
                    content += "\n" + currentTarget.GetAirframeType() + "  |  " + currentTarget.GetThreatLevel();
                else if (feedbackTimer > 0)
                    content += "\n" + feedback.Replace("press F or click", "press RIGHT TRIGGER");
                controls = state == GameState.Identifying ? "A: civilian     B: military     Y: level selection" :
                    "RIGHT TRIGGER: lock     LEFT STICK: move     RIGHT STICK: turn     Y: levels";
                break;
        }
        questHud.Show(title, content, controls, gameplay, gallery);
    }
}
