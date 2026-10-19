using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Stereo-visible UI operated with controller buttons rather than desktop GUI events.</summary>
public sealed class QuestHud : MonoBehaviour
{
    private RectTransform panel;
    private Image background;
    private TextMeshProUGUI title;
    private TextMeshProUGUI body;
    private TextMeshProUGUI footer;
    private RawImage preview;
    private GameObject reticle;

    public static QuestHud Create(Camera camera)
    {
        GameObject root = new GameObject("Quest interface", typeof(RectTransform), typeof(Canvas));
        root.transform.SetParent(camera.transform, false);
        root.transform.localPosition = new Vector3(0, 0, 2f);
        root.transform.localScale = Vector3.one * 0.0018f;
        Canvas canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = camera;
        canvas.sortingOrder = 100;
        RectTransform rect = (RectTransform)root.transform;
        rect.sizeDelta = new Vector2(1000, 680);
        QuestHud hud = root.AddComponent<QuestHud>();
        hud.Build();
        return hud;
    }

    private void Build()
    {
        panel = Box("Panel", transform, Vector2.zero, new Vector2(1000, 680));
        background = panel.gameObject.AddComponent<Image>();
        background.color = new Color(0.025f, 0.045f, 0.065f, 0.94f);
        background.raycastTarget = false;
        title = Label("Title", panel, new Vector2(0, 265), new Vector2(940, 70), 36);
        title.color = new Color(0.5f, 1f, 0.78f);
        body = Label("Body", panel, new Vector2(0, 20), new Vector2(920, 400), 29);
        footer = Label("Controls", panel, new Vector2(0, -270), new Vector2(940, 100), 25);
        footer.color = new Color(0.75f, 0.9f, 1f);
        preview = Box("Gallery", panel, new Vector2(-230, 25), new Vector2(440, 310)).gameObject.AddComponent<RawImage>();
        preview.raycastTarget = false;
        reticle = Label("Gaze reticle", transform, Vector2.zero, new Vector2(32, 32), 28).gameObject;
        reticle.GetComponent<TextMeshProUGUI>().text = "+";
        reticle.SetActive(false);
    }

    public void Show(string heading, string content, string controls, bool gameplay, Texture gallery = null)
    {
        title.text = heading;
        body.text = content;
        footer.text = controls;
        reticle.SetActive(gameplay);
        preview.gameObject.SetActive(gallery != null);
        preview.texture = gallery;
        panel.anchoredPosition = gameplay ? new Vector2(0, -290) : Vector2.zero;
        panel.sizeDelta = gameplay ? new Vector2(1000, 240) : new Vector2(1000, 680);
        title.rectTransform.anchoredPosition = new Vector2(0, gameplay ? 77 : 265);
        body.rectTransform.anchoredPosition = gallery != null ? new Vector2(230, 25) : new Vector2(0, gameplay ? 0 : 20);
        body.rectTransform.sizeDelta = new Vector2(gallery != null ? 440 : 920, gameplay ? 100 : 400);
        body.fontSize = gameplay ? 25 : 29;
        footer.rectTransform.anchoredPosition = new Vector2(0, gameplay ? -85 : -270);
        footer.rectTransform.sizeDelta = new Vector2(940, gameplay ? 50 : 100);
        footer.fontSize = gameplay ? 21 : 25;
    }

    private static RectTransform Box(string name, Transform parent, Vector2 position, Vector2 size)
    {
        RectTransform rect = (RectTransform)new GameObject(name, typeof(RectTransform)).transform;
        rect.SetParent(parent, false);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        return rect;
    }

    private static TextMeshProUGUI Label(string name, Transform parent, Vector2 position, Vector2 size, int fontSize)
    {
        TextMeshProUGUI text = Box(name, parent, position, size).gameObject.AddComponent<TextMeshProUGUI>();
        text.font = TMP_Settings.defaultFontAsset;
        text.fontSize = fontSize;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        text.raycastTarget = false;
        return text;
    }
}
