using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Video;
using System.IO;

[DefaultExecutionOrder(1000)]
public class DroneThreatSystem : MonoBehaviour
{
    private enum GameState { IntroVideo, MainMenu, Tutorial, Briefing, Playing, Locking, Identifying, LevelComplete }

    [Header("Engagement")]
    [Min(1f)] public float detectionTime = 10f;
    [Min(1f)] public float identificationTime = 10f;
    [Min(1f)] public float maxRange = 300f;
    [HideInInspector] public float scanTime = 1.5f; // Kept for existing scene setup scripts.

    [Header("References")]
    public Camera playerCamera;
    public int score;

    private GameState state = GameState.MainMenu;
    private TrainingDrone[] drones = new TrainingDrone[0];
    private TrainingDrone currentTarget;
    private FPSPlayerController playerController;
    private VideoPlayer tutorialVideoPlayer;
    private AudioSource tutorialAudioSource;
    private RenderTexture tutorialRenderTexture;
    private GameObject tutorialVideoCanvas;
    private float identifyTimer;
    private float lockTimer;
    private float feedbackTimer;
    private float introPrepareTimer;
    private string introFallbackPath;
    private string introVideoError = "";
    private bool triedIntroFallback;
    private int selectedLevel = -1;
    private int identifiedCount;
    private string feedback = "";
    private DroneBriefingScreen briefingScreen;
    private ScenePresentation scenePresentation;
    private VoiceGuidance voiceGuidance;

    private static readonly string[] LevelNames =
    {
        "01 | CIVILIAN TRACKING",
        "02 | MIXED AIRSPACE",
        "03 | FULL THREAT RESPONSE"
    };

    private void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        briefingScreen = GetComponent<DroneBriefingScreen>();
        if (briefingScreen == null) briefingScreen = gameObject.AddComponent<DroneBriefingScreen>();
        scenePresentation = GetComponent<ScenePresentation>();
        if (scenePresentation == null) scenePresentation = gameObject.AddComponent<ScenePresentation>();
        scenePresentation.Initialize(playerCamera);
        voiceGuidance = GetComponent<VoiceGuidance>();
        if (voiceGuidance == null) voiceGuidance = gameObject.AddComponent<VoiceGuidance>();

        GameObject player = GameObject.Find("TraineePlayer");
        if (player != null)
        {
            playerController = player.GetComponent<FPSPlayerController>();
            DroneScanner oldScanner = player.GetComponent<DroneScanner>();
            if (oldScanner != null)
                oldScanner.enabled = false;
        }

        GameObject droneRoot = GameObject.Find("DroneTraining");
        if (droneRoot != null)
            drones = droneRoot.GetComponentsInChildren<TrainingDrone>(true);

        for (int i = 0; i < drones.Length; i++)
        {
            DroneVisualModel visual = drones[i].GetComponent<DroneVisualModel>();
            if (visual == null) visual = drones[i].gameObject.AddComponent<DroneVisualModel>();
            visual.Initialize(drones[i].variant);
            drones[i].gameObject.SetActive(false);
        }

        GameObject legacyHud = GameObject.Find("TrainingHUD");
        if (legacyHud != null)
        {
            Transform oldCrosshair = legacyHud.transform.Find("Crosshair");
            Transform oldScanStatus = legacyHud.transform.Find("ScanStatus");
            Transform oldTargetStatus = legacyHud.transform.Find("TargetStatus");
            Transform oldControls = legacyHud.transform.Find("Controls");
            if (oldCrosshair != null) oldCrosshair.gameObject.SetActive(false);
            if (oldScanStatus != null) oldScanStatus.gameObject.SetActive(false);
            if (oldTargetStatus != null) oldTargetStatus.gameObject.SetActive(false);
            if (oldControls != null) oldControls.gameObject.SetActive(false);
        }

        SetPlayerInput(false);
        state = GameState.IntroVideo;
        StartIntroVideo();
    }

    private void Update()
    {
        if (voiceGuidance != null) voiceGuidance.AllowReminders = state == GameState.Playing;
        if (state == GameState.IntroVideo)
        {
            introPrepareTimer += Time.unscaledDeltaTime;
            if (string.IsNullOrEmpty(introVideoError) && tutorialVideoPlayer != null &&
                !tutorialVideoPlayer.isPrepared && introPrepareTimer >= 20f)
            {
                TryIntroVideoFallback("The intro video did not prepare within 20 seconds.");
            }
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                FinishIntroVideo();
            return;
        }

        if (feedbackTimer > 0f)
            feedbackTimer -= Time.deltaTime;

        if (state == GameState.Playing)
        {
            if (WasIdentifyPressed())
            {
                TrainingDrone aimed = GetAimedDrone();
                if (aimed != null)
                    BeginLock(aimed);
                else
                {
                    ShowFeedback("Aim at a drone, then press F or click to lock aim.");
                }
            }
        }
        else if (state == GameState.Locking)
        {
            if (currentTarget == null || !currentTarget.gameObject.activeInHierarchy || currentTarget.IsDowned)
            {
                currentTarget = null;
                state = GameState.Playing;
                return;
            }

            if (GetAimedDrone() != currentTarget)
            {
                currentTarget = null;
                lockTimer = 0f;
                state = GameState.Playing;
                ShowFeedback("Aim lock lost. Center the drone and lock again.");
                return;
            }

            lockTimer += Time.deltaTime;
            if (lockTimer >= detectionTime)
                DownTarget();
        }
        else if (state == GameState.Identifying)
        {
            if (currentTarget == null || !currentTarget.gameObject.activeInHierarchy)
            {
                currentTarget = null;
                state = GameState.Playing;
                return;
            }

            identifyTimer -= Time.deltaTime;
            if (identifyTimer <= 0f)
                IdentificationExpired();
            else if (Keyboard.current != null)
            {
                if (Keyboard.current.jKey.wasPressedThisFrame)
                    AttemptIdentification(TrainingDrone.DroneClass.Cheap);
                else if (Keyboard.current.kKey.wasPressedThisFrame)
                    AttemptIdentification(TrainingDrone.DroneClass.Military);
            }
        }
    }

    private bool WasIdentifyPressed()
    {
        return Keyboard.current != null &&
            (Keyboard.current.fKey.wasPressedThisFrame ||
             Keyboard.current.eKey.wasPressedThisFrame ||
             (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame));
    }

    private TrainingDrone GetAimedDrone()
    {
        if (playerCamera == null)
            return null;

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (Physics.Raycast(ray, out RaycastHit hit, maxRange,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            TrainingDrone drone = hit.collider.GetComponentInParent<TrainingDrone>();
            if (drone != null && !drone.IsDowned && drone.gameObject.activeInHierarchy)
                return drone;
        }

        return null;
    }

    private void BeginLock(TrainingDrone drone)
    {
        currentTarget = drone;
        lockTimer = 0f;
        state = GameState.Locking;
        ShowFeedback("LOCKING — KEEP THE DRONE CENTERED FOR 10 SECONDS");
    }

    private void DownTarget()
    {
        if (currentTarget == null)
            return;

        currentTarget.DownForIdentification();
        identifyTimer = identificationTime;
        state = GameState.Identifying;
        ShowFeedback("TARGET DOWN — IDENTIFY WITHIN 10 SECONDS");
    }

    private void AttemptIdentification(TrainingDrone.DroneClass chosenClass)
    {
        if (currentTarget == null)
            return;

        bool correctlyIdentified = chosenClass == currentTarget.droneClass;

        if (!correctlyIdentified)
        {
            score = Mathf.Max(0, score - 50);
            ShowFeedback("Incorrect class. Try again before the threat timer expires.");
            return;
        }

        score += 100;
        identifiedCount++;
        currentTarget.gameObject.SetActive(false);
        currentTarget = null;
        if (identifiedCount >= GetLevelDroneCount(selectedLevel))
        {
            state = GameState.LevelComplete;
            SetPlayerInput(false);
            voiceGuidance.SetContext(VoiceGuidance.Context.Complete);
        }
        else
        {
            state = GameState.Playing;
            voiceGuidance.ReportProgress();
        }
        ShowFeedback("IDENTIFIED AND NEUTRALIZED — +100 POINTS");
    }

    private void IdentificationExpired()
    {
        TrainingDrone missedTarget = currentTarget;
        currentTarget = null;
        state = GameState.Playing;
        if (missedTarget != null)
        {
            missedTarget.ResetForRound();
            missedTarget.gameObject.SetActive(true);
        }
        ShowFeedback("IDENTIFICATION WINDOW CLOSED — TRY AGAIN");
    }

    private void OpenBriefing(int level)
    {
        SetPlayerInput(false);
        state = GameState.Briefing;
        voiceGuidance.SetContext(VoiceGuidance.Context.Gallery);
        briefingScreen.Show(level, () => SelectLevel(level), ReturnToMenu);
    }

    private void SelectLevel(int level)
    {
        if (drones.Length == 0)
        {
            GameObject droneRoot = GameObject.Find("DroneTraining");
            if (droneRoot != null)
                drones = droneRoot.GetComponentsInChildren<TrainingDrone>(true);
        }

        if (drones.Length == 0)
        {
            ReturnToMenu();
            ShowFeedback("No drones found. Let the scene setup finish, then reopen the project.");
            return;
        }

        selectedLevel = Mathf.Clamp(level, 0, LevelNames.Length - 1);
        identifiedCount = 0;
        score = 0;
        currentTarget = null;
        for (int i = 0; i < drones.Length; i++)
        {
            drones[i].ResetForRound();
            // Reassign on every round: older scenes have no serialized slot data.
            drones[i].ConfigureAI(playerController != null ? playerController.transform :
                playerCamera != null ? playerCamera.transform : null, i);
            drones[i].gameObject.SetActive(i < GetLevelDroneCount(selectedLevel));
        }

        state = GameState.Playing;
        SetPlayerInput(true);
        voiceGuidance.SetContext(VoiceGuidance.Context.Gameplay);
    }

    private void ReturnToMenu()
    {
        if (briefingScreen != null) briefingScreen.Close();
        currentTarget = null;
        for (int i = 0; i < drones.Length; i++)
            drones[i].gameObject.SetActive(false);
        state = GameState.MainMenu;
        SetPlayerInput(false);
        voiceGuidance.SetContext(VoiceGuidance.Context.Menu);
    }

    private static int GetLevelDroneCount(int level)
    {
        return level == 0 ? 3 : level == 1 ? 4 : 6;
    }

    private void StartIntroVideo()
    {
        bool useLinuxVideo = Application.platform == RuntimePlatform.LinuxEditor ||
                             Application.platform == RuntimePlatform.LinuxPlayer;
        string videoFile = useLinuxVideo
            ? "DroneThreatSimulator-HowToPlay.webm"
            : "DroneThreatSimulator-HowToPlay.mp4";
        string videoPath = Path.Combine(Application.streamingAssetsPath, videoFile);
        introFallbackPath = Path.Combine(Application.streamingAssetsPath,
            useLinuxVideo ? "DroneThreatSimulator-HowToPlay.mp4" : "DroneThreatSimulator-HowToPlay.webm");
        triedIntroFallback = false;
        introVideoError = "";
        // StreamingAssets is a URL on some platforms, where File.Exists fails.
        if (!videoPath.Contains("://") && !File.Exists(videoPath))
        {
            Debug.LogWarning("Tutorial video not found at " + videoPath + ". Showing mission select instead.");
            state = GameState.MainMenu;
            voiceGuidance.SetContext(VoiceGuidance.Context.Menu);
            return;
        }

        tutorialRenderTexture = new RenderTexture(1280, 720, 0, RenderTextureFormat.ARGB32);
        tutorialRenderTexture.Create();

        tutorialVideoCanvas = new GameObject("TutorialVideoCanvas");
        Canvas canvas = tutorialVideoCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 500;
        CanvasScaler scaler = tutorialVideoCanvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        tutorialVideoCanvas.AddComponent<GraphicRaycaster>();

        GameObject imageObject = new GameObject("TutorialVideoImage");
        imageObject.transform.SetParent(tutorialVideoCanvas.transform, false);
        RectTransform rect = imageObject.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        RawImage image = imageObject.AddComponent<RawImage>();
        image.texture = tutorialRenderTexture;

        tutorialAudioSource = gameObject.AddComponent<AudioSource>();
        tutorialAudioSource.playOnAwake = false;
        tutorialAudioSource.spatialBlend = 0f;

        tutorialVideoPlayer = gameObject.AddComponent<VideoPlayer>();
        tutorialVideoPlayer.playOnAwake = false;
        tutorialVideoPlayer.isLooping = false;
        tutorialVideoPlayer.source = VideoSource.Url;
        // Unity's VideoPlayer accepts the absolute StreamingAssets path directly.
        tutorialVideoPlayer.url = videoPath;
        tutorialVideoPlayer.waitForFirstFrame = true;
        tutorialVideoPlayer.skipOnDrop = false;
        tutorialVideoPlayer.renderMode = VideoRenderMode.RenderTexture;
        tutorialVideoPlayer.targetTexture = tutorialRenderTexture;
        tutorialVideoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        tutorialVideoPlayer.prepareCompleted += OnTutorialVideoPrepared;
        tutorialVideoPlayer.loopPointReached += OnTutorialVideoFinished;
        tutorialVideoPlayer.errorReceived += OnTutorialVideoError;
        tutorialVideoPlayer.Prepare();
    }

    private void OnTutorialVideoPrepared(VideoPlayer source)
    {
        if (source.audioTrackCount > 0)
        {
            source.EnableAudioTrack(0, true);
            source.SetTargetAudioSource(0, tutorialAudioSource);
        }
        Debug.Log("Starting intro video: " + source.url);
        source.Play();
    }

    private void OnTutorialVideoFinished(VideoPlayer source)
    {
        FinishIntroVideo();
    }

    private void OnTutorialVideoError(VideoPlayer source, string message)
    {
        TryIntroVideoFallback("Could not play " + source.url + ": " + message);
    }

    private void TryIntroVideoFallback(string reason)
    {
        if (tutorialVideoPlayer != null && !triedIntroFallback &&
            (introFallbackPath.Contains("://") || File.Exists(introFallbackPath)))
        {
            triedIntroFallback = true;
            introPrepareTimer = 0f;
            tutorialVideoPlayer.Stop();
            tutorialVideoPlayer.url = introFallbackPath;
            Debug.LogWarning(reason + " Trying the alternate intro video: " + introFallbackPath);
            tutorialVideoPlayer.Prepare();
            return;
        }

        introVideoError = reason + " Check the Unity Console for details, or press Esc to continue.";
        Debug.LogError(introVideoError);
    }

    private void FinishIntroVideo()
    {
        if (tutorialVideoPlayer != null)
        {
            tutorialVideoPlayer.prepareCompleted -= OnTutorialVideoPrepared;
            tutorialVideoPlayer.loopPointReached -= OnTutorialVideoFinished;
            tutorialVideoPlayer.errorReceived -= OnTutorialVideoError;
            tutorialVideoPlayer.Stop();
            Destroy(tutorialVideoPlayer);
            tutorialVideoPlayer = null;
        }

        if (tutorialVideoCanvas != null)
            Destroy(tutorialVideoCanvas);
        if (tutorialRenderTexture != null)
        {
            tutorialRenderTexture.Release();
            Destroy(tutorialRenderTexture);
        }
        if (tutorialAudioSource != null)
            Destroy(tutorialAudioSource);

        state = GameState.MainMenu;
        SetPlayerInput(false);
        voiceGuidance.SetContext(VoiceGuidance.Context.Menu);
    }

    private void SetPlayerInput(bool enabled)
    {
        if (playerController != null)
            playerController.SetGameplayInputEnabled(enabled);
        else if (!enabled)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void ShowFeedback(string message)
    {
        feedback = message;
        feedbackTimer = 2.2f;
    }

    private void OnGUI()
    {
        if (state == GameState.Briefing)
        {
            briefingScreen.Draw();
            return;
        }

        if (state == GameState.IntroVideo)
        {
            DrawIntroVideoControls();
            return;
        }

        if (state == GameState.MainMenu)
        {
            DrawMainMenu();
            return;
        }

        if (state == GameState.Tutorial)
        {
            DrawTutorial();
            return;
        }

        if (state == GameState.LevelComplete)
        {
            DrawLevelComplete();
            return;
        }

        DrawCrosshair();
        DrawHud();
        if (state == GameState.Identifying)
            DrawIdentificationPanel();
        if (feedbackTimer > 0f)
            DrawFeedback();
    }

    private void DrawIntroVideoControls()
    {
        if (!string.IsNullOrEmpty(introVideoError))
        {
            GUI.Box(new Rect(Screen.width * 0.5f - 320f, Screen.height * 0.5f - 58f, 640f, 116f), GUIContent.none);
            GUI.Label(new Rect(Screen.width * 0.5f - 300f, Screen.height * 0.5f - 45f, 600f, 74f),
                "INTRO VIDEO COULD NOT PLAY\n" + introVideoError, CenteredStyle(18, true));
        }
        else if (tutorialVideoPlayer == null || !tutorialVideoPlayer.isPrepared)
        {
            GUIStyle loadingStyle = CenteredStyle(20, true);
            GUI.Label(new Rect(Screen.width * 0.5f - 260f, Screen.height - 76f, 520f, 36f),
                "LOADING INTRO VIDEO...", loadingStyle);
        }
        Rect skip = new Rect(Screen.width - 176f, 24f, 148f, 44f);
        if (GUI.Button(skip, "SKIP TUTORIAL  [ ESC ]"))
            FinishIntroVideo();
    }

    private void DrawMainMenu()
    {
        Matrix4x4 previous = PresentationGUI.Begin();
        GUI.Label(new Rect(56, 34, 1100, 24), "SIMULATION LAB  /  SCENARIO SELECT", PresentationGUI.Caption);
        GUI.Label(new Rect(56, 80, 1150, 56), "Drone Threat Simulator", PresentationGUI.Title);
        GUI.Label(new Rect(56, 145, 1000, 55), "Choose a level, explore the airframe gallery, then enter the environment.", PresentationGUI.Body);
        if (GUI.Button(new Rect(974, 34, 250, 42), "HOW TO PLAY", PresentationGUI.Button))
            state = GameState.Tutorial;

        int requestedLevel = -1;
        for (int i = 0; i < LevelNames.Length; i++)
        {
            float x = 56 + i * 393;
            PresentationGUI.Fill(new Rect(x, 236, 382, 300), PresentationGUI.Panel);
            PresentationGUI.Fill(new Rect(x, 236, 382, 3), PresentationGUI.Accent);
            GUI.Label(new Rect(x + 24, 263, 320, 30), "SCENARIO  /  0" + (i + 1), PresentationGUI.Caption);
            GUI.Label(new Rect(x + 24, 313, 332, 65), LevelNames[i].Substring(5), PresentationGUI.Heading);
            GUI.Label(new Rect(x + 24, 397, 332, 60), "Existing level  •  " + GetLevelDroneCount(i) + " aircraft\nIncludes pre-level 3D gallery", PresentationGUI.Body);
            if (GUI.Button(new Rect(x + 24, 470, 334, 44), "EXPLORE LEVEL " + (i + 1), PresentationGUI.Button))
                requestedLevel = i;
        }
        GUI.Label(new Rect(56, 575, 350, 25), "VISUAL PRESENTATION", PresentationGUI.Caption);
        string[] modes = { "Original", "Performance", "Enhanced" };
        for (int i = 0; i < modes.Length; i++)
        {
            bool active = (int)scenePresentation.Mode == i;
            if (GUI.Button(new Rect(56 + i * 180, 611, 168, 42), (active ? "• " : "") + modes[i], PresentationGUI.Button))
                scenePresentation.SetMode((ScenePresentation.VisualMode)i);
        }
        GUI.Label(new Rect(635, 590, 580, 65), "Enhanced: daylight, sky fill, soft shadows and color grading.\nPerformance: disables post-processing and local light shadows.", PresentationGUI.Body);
        if (feedbackTimer > 0f)
            GUI.Label(new Rect(56, 675, 1168, 30), feedback, PresentationGUI.Caption);
        GUI.matrix = previous;
        if (requestedLevel >= 0) OpenBriefing(requestedLevel);
    }

    private void DrawTutorial()
    {
        Rect panel = CenterPanel(700f, 500f);
        GUI.Box(panel, GUIContent.none, GUI.skin.window);
        GUILayout.BeginArea(new Rect(panel.x + 36f, panel.y + 30f, panel.width - 72f, panel.height - 60f));
        GUILayout.Label("FIELD MANUAL", TitleStyle());
        GUILayout.Label("MISSION FLOW", HeaderStyle());
        GUILayout.Space(16f);
        GUILayout.Label("1. Choose a mission. Move with WASD and aim with the mouse.", BodyStyle());
        GUILayout.Label("2. Center a drone in your sights and press F or click to lock aim for 10 seconds.", BodyStyle());
        GUILayout.Label("3. When it is down, identify it within 10 seconds: J for civilian or K for military.", BodyStyle());
        GUILayout.Label("4. Correct identification scores 100 points. Complete the level objectives.", BodyStyle());
        GUILayout.Space(12f);
        GUILayout.Label("LOCK AIM = F / LEFT CLICK     |     CIVILIAN = J     |     MILITARY = K", HeaderStyle());
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("BACK TO MISSIONS", GUILayout.Height(46f)))
            state = GameState.MainMenu;
        GUILayout.EndArea();
    }

    private void DrawLevelComplete()
    {
        Rect panel = CenterPanel(560f, 300f);
        GUI.Box(panel, GUIContent.none, GUI.skin.window);
        GUILayout.BeginArea(new Rect(panel.x + 36f, panel.y + 28f, panel.width - 72f, panel.height - 56f));
        GUILayout.Label("MISSION COMPLETE", TitleStyle());
        GUILayout.Label(LevelNames[Mathf.Clamp(selectedLevel, 0, LevelNames.Length - 1)], HeaderStyle());
        GUILayout.Label("Level complete  |  Score: " + score, BodyStyle());
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("REPLAY MISSION", GUILayout.Height(44f)))
            OpenBriefing(selectedLevel);
        if (GUILayout.Button("MISSION SELECT", GUILayout.Height(40f)))
            ReturnToMenu();
        GUILayout.EndArea();
    }

    private void DrawHud()
    {
        GUI.Box(new Rect(18f, 18f, 320f, 82f), "");
        GUI.Label(new Rect(32f, 27f, 296f, 28f), LevelNames[Mathf.Clamp(selectedLevel, 0, LevelNames.Length - 1)], HeaderStyle());
        GUI.Label(new Rect(32f, 57f, 296f, 28f), "SCORE " + score + "     CLASSIFIED " + identifiedCount, BodyStyle());
        GUI.Label(new Rect(32f, 101f, 360f, 24f), "VISUALS: " + scenePresentation.Mode + "  |  F8 COMPARE GRAPHICS", CenteredStyle(12, false));

        string title;
        string hint;
        if (state == GameState.Locking)
        {
            title = "AIM LOCK  " + Mathf.Max(0f, detectionTime - lockTimer).ToString("0.0") + " s";
            hint = "KEEP THE DRONE CENTERED";
        }
        else if (state == GameState.Identifying)
        {
            title = "THREAT TIMER  " + Mathf.Max(0f, identifyTimer).ToString("0.0") + " s";
            hint = "CLASSIFY NOW  |  J: CIVILIAN   K: MILITARY";
        }
        else
        {
            title = "READY FOR TARGET";
            hint = "CENTER A DRONE, THEN LOCK AIM";
        }

        bool narrow = Screen.width < 900;
        float statusX = narrow ? 18f : Mathf.Max(354f, Screen.width * 0.5f - 270f);
        float statusY = narrow ? 132f : 18f;
        float statusWidth = Mathf.Min(540f, Screen.width - statusX - 18f);
        PresentationGUI.Fill(new Rect(statusX, statusY, statusWidth, 78f), new Color(0.025f, 0.04f, 0.06f, 0.88f));
        GUI.Label(new Rect(statusX + 8, statusY + 6, statusWidth - 16, 38f), title, CenteredStyle(23, true));
        GUI.Label(new Rect(statusX + 8, statusY + 42, statusWidth - 16, 30f), hint, CenteredStyle(15, false));

        if (state == GameState.Playing)
        {
            TrainingDrone aimed = GetAimedDrone();
            Rect lockRect = new Rect(Screen.width * 0.5f - 125f, Screen.height - 112f, 250f, 52f);
            GUI.enabled = aimed != null;
            if (GUI.Button(lockRect, aimed == null ? "AIM AT A DRONE" : "LOCK AIM  [ F ]") && aimed != null)
                BeginLock(aimed);
            GUI.enabled = true;
        }

        if (state == GameState.Locking)
            GUI.Label(new Rect(Screen.width * 0.5f - 270f, Screen.height - 88f, 540f, 36f),
                "LOCK PROGRESS  " + (lockTimer / detectionTime * 100f).ToString("0") + "%", CenteredStyle(18, true));

        if (state == GameState.Identifying && currentTarget != null)
            DrawDroneClues(currentTarget);
    }

    private void DrawIdentificationPanel()
    {
        float width = Mathf.Min(620f, Screen.width - 32f);
        Rect panel = new Rect((Screen.width - width) * 0.5f, Screen.height - 235f, width, 120f);
        GUI.Box(panel, "IDENTIFICATION", GUI.skin.window);
        float buttonWidth = (width - 54f) * 0.5f;
        if (GUI.Button(new Rect(panel.x + 18f, panel.y + 42f, buttonWidth, 54f), "CIVILIAN  [ J ]"))
            AttemptIdentification(TrainingDrone.DroneClass.Cheap);
        if (GUI.Button(new Rect(panel.x + 36f + buttonWidth, panel.y + 42f, buttonWidth, 54f), "MILITARY  [ K ]"))
            AttemptIdentification(TrainingDrone.DroneClass.Military);
    }

    private void DrawDroneClues(TrainingDrone drone)
    {
        Rect panel = new Rect(Screen.width - 330f, 118f, 310f, 110f);
        GUI.Box(panel, "DRONE SIGNATURE", GUI.skin.window);
        GUI.Label(new Rect(panel.x + 15f, panel.y + 36f, 280f, 26f), "AIRFRAME: " + drone.GetAirframeType(), BodyStyle());
        GUI.Label(new Rect(panel.x + 15f, panel.y + 66f, 280f, 26f), "SIGNATURE: " + drone.GetThreatLevel(), BodyStyle());
    }

    private void DrawCrosshair()
    {
        GUI.Label(new Rect(Screen.width * 0.5f - 18f, Screen.height * 0.5f - 18f, 36f, 36f), "+", CenteredStyle(28, true));
    }

    private void DrawFeedback()
    {
        Rect rect = new Rect(Screen.width * 0.5f - 330f, 100f, 660f, 42f);
        GUI.Box(rect, feedback, CenteredStyle(17, true));
    }

    private static Rect CenterPanel(float width, float height)
    {
        width = Mathf.Min(width, Screen.width - 32f);
        height = Mathf.Min(height, Screen.height - 88f);
        return new Rect((Screen.width - width) * 0.5f, (Screen.height - 56f - height) * 0.5f, width, height);
    }

    private static GUIStyle TitleStyle()
    {
        return new GUIStyle(GUI.skin.label) { fontSize = 28, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, normal = { textColor = new Color(0.65f, 1f, 0.72f) } };
    }

    private static GUIStyle HeaderStyle()
    {
        return new GUIStyle(GUI.skin.label) { fontSize = 18, fontStyle = FontStyle.Bold, normal = { textColor = Color.white } };
    }

    private static GUIStyle BodyStyle()
    {
        return new GUIStyle(GUI.skin.label) { fontSize = 15, wordWrap = true, normal = { textColor = new Color(0.9f, 0.93f, 0.9f) } };
    }

    private static GUIStyle CenteredStyle(int size, bool bold)
    {
        return new GUIStyle(GUI.skin.label) { fontSize = size, fontStyle = bold ? FontStyle.Bold : FontStyle.Normal, alignment = TextAnchor.MiddleCenter, normal = { textColor = Color.white } };
    }
}
