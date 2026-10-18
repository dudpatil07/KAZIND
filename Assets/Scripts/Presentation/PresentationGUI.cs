using UnityEngine;

/// <summary>Shared, allocation-free-after-initialization styling for menu screens.</summary>
public static class PresentationGUI
{
    public static readonly Color Background = new Color(0.025f, 0.045f, 0.065f);
    public static readonly Color Panel = new Color(0.055f, 0.085f, 0.11f);
    public static readonly Color Accent = new Color(0.43f, 0.87f, 0.83f);
    public static GUIStyle Title { get; private set; }
    public static GUIStyle Heading { get; private set; }
    public static GUIStyle Body { get; private set; }
    public static GUIStyle Caption { get; private set; }
    public static GUIStyle Button { get; private set; }

    public static Matrix4x4 Begin()
    {
        EnsureStyles();
        Matrix4x4 previous = GUI.matrix;
        Fill(new Rect(0, 0, Screen.width, Screen.height), Background);
        // Keep menu buttons above the accessible narration/subtitle footer.
        float availableHeight = Mathf.Max(1, Screen.height - 56f);
        float scale = Mathf.Min(Screen.width / 1280f, availableHeight / 720f);
        GUI.matrix = Matrix4x4.TRS(new Vector3((Screen.width - 1280f * scale) * 0.5f,
            (availableHeight - 720f * scale) * 0.5f), Quaternion.identity, Vector3.one * scale);
        return previous;
    }

    public static void Fill(Rect rect, Color color)
    {
        Color previous = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = previous;
    }

    private static void EnsureStyles()
    {
        if (Title != null) return;
        Title = Label(36, Color.white, true);
        Heading = Label(23, Color.white, true);
        Body = Label(17, new Color(0.76f, 0.82f, 0.85f), false);
        Caption = Label(13, Accent, true);
        Button = new GUIStyle(GUI.skin.button) { fontSize = 16, fontStyle = FontStyle.Bold };
        Button.padding = new RectOffset(14, 14, 10, 10);
    }

    private static GUIStyle Label(int size, Color color, bool bold)
    {
        return new GUIStyle(GUI.skin.label)
        {
            fontSize = size, wordWrap = true,
            fontStyle = bold ? FontStyle.Bold : FontStyle.Normal,
            normal = { textColor = color }
        };
    }
}
