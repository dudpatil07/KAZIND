using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

[DisallowMultipleComponent]
public sealed class DroneBriefingScreen : MonoBehaviour
{
    private static readonly string[] Names = { "Compact quadcopter", "Fixed-wing aircraft", "Drone formation" };
    private static readonly string[] Profiles =
    {
        "MULTIROTOR  /  SINGLE AIRFRAME\n\nFour propellers, a compact central body and a small camera housing. An illustrative consumer-style design.",
        "FIXED WING  /  SINGLE AIRFRAME\n\nA continuous wing, streamlined fuselage and separate tail surfaces. An illustrative aircraft design.",
        "MULTIROTOR  /  GROUP DISPLAY\n\nFive lightweight quadcopters arranged in a repeating formation. This display is a visual concept, not a flight simulation."
    };
    private GameObject stage;
    private Camera previewCamera;
    private RenderTexture texture;
    private readonly GameObject[] models = new GameObject[3];
    private readonly Material[] materials = new Material[3];
    private int selected;
    private int levelNumber;
    private Action onContinue;
    private Action onCancel;
    public bool IsOpen { get; private set; }
    public Texture PreviewTexture => texture;
    public string SelectedName => Names[selected];
    public string SelectedProfile => Profiles[selected];
    public void CycleModel(int direction) => Select((selected + direction + models.Length) % models.Length);
    public void ContinueFromVR() => Finish(true);

    public void Show(int level, Action continueAction, Action cancelAction)
    {
        EnsureStage();
        levelNumber = level + 1;
        onContinue = continueAction;
        onCancel = cancelAction;
        IsOpen = true;
        if (stage != null) stage.SetActive(true);
        Select(0);
    }

    private void EnsureStage()
    {
        if (stage != null) return;
        Shader shader = Resources.Load<Shader>("Presentation/Showcase");
        if (shader == null)
        {
            Debug.LogWarning("Showcase shader unavailable. The briefing will use text only.");
            return;
        }
        Color[] colors = { new Color(0.67f, 0.7f, 0.69f), new Color(0.08f, 0.1f, 0.12f), PresentationGUI.Accent };
        for (int i = 0; i < materials.Length; i++)
        {
            materials[i] = new Material(shader) { name = "ShowcaseMaterial", hideFlags = HideFlags.DontSave };
            materials[i].SetColor("_BaseColor", colors[i]);
        }

        stage = new GameObject("PresentationStage") { hideFlags = HideFlags.DontSave };
        // Remote studio plus a short camera far plane isolates the scene without modifying gameplay cameras.
        stage.transform.position = new Vector3(0, -10000, 0);
        for (int i = 0; i < models.Length; i++)
            models[i] = DroneShowcaseModels.Build(i, stage.transform, materials[0], materials[1], materials[2]);

        GameObject cameraObject = new GameObject("ShowcaseCamera");
        cameraObject.transform.SetParent(stage.transform, false);
        cameraObject.transform.localPosition = new Vector3(0, 2.4f, -5.6f);
        cameraObject.transform.LookAt(stage.transform.position);
        previewCamera = cameraObject.AddComponent<Camera>();
        previewCamera.cullingMask = 1 << 31;
        previewCamera.clearFlags = CameraClearFlags.SolidColor;
        previewCamera.backgroundColor = PresentationGUI.Panel;
        previewCamera.fieldOfView = 42;
        previewCamera.nearClipPlane = 0.1f;
        previewCamera.farClipPlane = 15;
        previewCamera.allowHDR = false;
        previewCamera.allowMSAA = false;
        previewCamera.stereoTargetEye = StereoTargetEyeMask.None;
        UniversalAdditionalCameraData cameraData = cameraObject.AddComponent<UniversalAdditionalCameraData>();
        cameraData.renderPostProcessing = false;
        cameraData.renderShadows = false;
        cameraData.requiresColorOption = CameraOverrideOption.Off;
        cameraData.requiresDepthOption = CameraOverrideOption.Off;
        texture = new RenderTexture(768, 512, 16, RenderTextureFormat.ARGB32)
            { name = "DroneShowcase", hideFlags = HideFlags.DontSave };
        texture.Create();
        previewCamera.targetTexture = texture;
    }

    private void Select(int index)
    {
        selected = Mathf.Clamp(index, 0, 2);
        for (int i = 0; i < models.Length; i++)
            if (models[i] != null) models[i].SetActive(i == selected);
    }

    private void Update()
    {
        if (!IsOpen) return;
        if (models[selected] != null)
            models[selected].transform.Rotate(Vector3.up, 14f * Time.unscaledDeltaTime, Space.Self);
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            Finish(false);
    }

    public void Draw()
    {
        if (!IsOpen) return;
        Matrix4x4 previous = PresentationGUI.Begin();
        GUI.Label(new Rect(56, 34, 900, 24), "AIRFRAME GALLERY  /  BEFORE LEVEL " + levelNumber, PresentationGUI.Caption);
        GUI.Label(new Rect(56, 72, 1100, 50), "Explore the aircraft", PresentationGUI.Title);
        GUI.Label(new Rect(56, 131, 1100, 35), "Three illustrative 3D designs. Your selected level uses its existing aircraft roster.", PresentationGUI.Body);
        for (int i = 0; i < 3; i++)
        {
            Rect tab = new Rect(56 + i * 393, 187, 382, 48);
            if (GUI.Button(tab, "0" + (i + 1) + "   " + Names[i], PresentationGUI.Button)) Select(i);
            if (selected == i) PresentationGUI.Fill(new Rect(tab.x, tab.yMax + 3, tab.width, 3), PresentationGUI.Accent);
        }
        Rect preview = new Rect(56, 258, 660, 352);
        PresentationGUI.Fill(preview, PresentationGUI.Panel);
        if (texture != null) GUI.DrawTexture(preview, texture, ScaleMode.ScaleToFit, false);
        else GUI.Label(new Rect(80, 400, 600, 40), "3D preview unavailable — see the Unity Console.", PresentationGUI.Body);
        GUI.Label(new Rect(748, 277, 465, 45), Names[selected], PresentationGUI.Heading);
        GUI.Label(new Rect(748, 340, 450, 210), Profiles[selected], PresentationGUI.Body);
        GUI.Label(new Rect(748, 563, 450, 45), "GENERIC VISUAL MODEL  /  NOT A REAL SYSTEM", PresentationGUI.Caption);
        bool cancel = GUI.Button(new Rect(56, 642, 230, 46), "BACK TO LEVELS", PresentationGUI.Button);
        bool proceed = GUI.Button(new Rect(974, 642, 250, 46), "START LEVEL " + levelNumber, PresentationGUI.Button);
        GUI.matrix = previous;
        if (cancel || proceed) Finish(proceed);
    }

    private void Finish(bool proceed)
    {
        Action callback = proceed ? onContinue : onCancel;
        Close();
        callback?.Invoke();
    }

    public void Close()
    {
        IsOpen = false;
        if (stage != null) stage.SetActive(false);
        onContinue = null;
        onCancel = null;
    }

    private void OnDisable() => Close();

    private void OnDestroy()
    {
        if (previewCamera != null) previewCamera.targetTexture = null;
        if (texture != null) { texture.Release(); Destroy(texture); }
        if (stage != null) Destroy(stage);
        foreach (Material material in materials) if (material != null) Destroy(material);
    }
}
