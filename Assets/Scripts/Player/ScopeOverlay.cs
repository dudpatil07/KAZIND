using UnityEngine;

public class ScopeOverlay : MonoBehaviour
{
    [Header("Scope")]
    public FPSPlayerController player;

    public bool showScopeInfo = true;

    [Header("Appearance")]
    public float scopeSize = 0.82f;
    public float reticleThickness = 2f;
    public float vignetteAlpha = 0.88f;

    private Texture2D maskTexture;
    private Texture2D whiteTexture;

    private GUIStyle textStyle;
    private GUIStyle smallStyle;

    void Start()
    {
        if (player == null)
            player =
                GetComponentInParent<FPSPlayerController>();

        if (player == null)
            player =
                FindFirstObjectByType<FPSPlayerController>();

        CreateTextures();
        CreateStyles();
    }

    void CreateTextures()
    {
        // --------------------------------------------------------
        // Circular scope mask
        // Transparent inside the circle
        // Dark outside
        // --------------------------------------------------------

        const int size = 512;

        maskTexture =
            new Texture2D(
                size,
                size,
                TextureFormat.RGBA32,
                false
            );

        maskTexture.wrapMode =
            TextureWrapMode.Clamp;

        Vector2 center =
            new Vector2(
                size * 0.5f,
                size * 0.5f
            );

        float radius =
            size * 0.46f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx =
                    x - center.x;

                float dy =
                    y - center.y;

                float distance =
                    Mathf.Sqrt(
                        dx * dx +
                        dy * dy
                    );

                if (distance <= radius)
                {
                    // Transparent scope glass
                    maskTexture.SetPixel(
                        x,
                        y,
                        new Color(
                            0f,
                            0f,
                            0f,
                            0f
                        )
                    );
                }
                else
                {
                    maskTexture.SetPixel(
                        x,
                        y,
                        new Color(
                            0f,
                            0f,
                            0f,
                            vignetteAlpha
                        )
                    );
                }
            }
        }

        maskTexture.Apply();

        // --------------------------------------------------------
        // 1x1 white texture
        // Used for reticle lines
        // --------------------------------------------------------

        whiteTexture =
            new Texture2D(
                1,
                1,
                TextureFormat.RGBA32,
                false
            );

        whiteTexture.SetPixel(
            0,
            0,
            Color.white
        );

        whiteTexture.Apply();
    }

    void CreateStyles()
    {
        textStyle =
            new GUIStyle();

        textStyle.fontSize =
            18;

        textStyle.fontStyle =
            FontStyle.Bold;

        textStyle.normal.textColor =
            new Color(
                0.45f,
                1f,
                0.55f,
                0.85f
            );

        smallStyle =
            new GUIStyle();

        smallStyle.fontSize =
            13;

        smallStyle.normal.textColor =
            new Color(
                0.45f,
                1f,
                0.55f,
                0.75f
            );
    }

    void OnGUI()
    {
        if (player == null)
            return;

        if (!player.IsScoped())
            return;

        DrawScope();

        DrawReticle();

        if (showScopeInfo)
            DrawScopeInfo();
    }

    // ============================================================
    // SCOPE MASK
    // ============================================================

    void DrawScope()
    {
        float size =
            Mathf.Min(
                Screen.width,
                Screen.height
            ) * scopeSize;

        Rect rect =
            new Rect(
                (Screen.width - size) * 0.5f,
                (Screen.height - size) * 0.5f,
                size,
                size
            );

        GUI.color =
            Color.white;

        GUI.DrawTexture(
            rect,
            maskTexture,
            ScaleMode.StretchToFill
        );

        // dark bars outside the optic
        GUI.color =
            new Color(
                0f,
                0f,
                0f,
                0.92f
            );

        float top =
            rect.y;

        float bottom =
            Screen.height -
            rect.y -
            rect.height;

        float left =
            rect.x;

        float right =
            Screen.width -
            rect.x -
            rect.width;

        GUI.DrawTexture(
            new Rect(
                0f,
                0f,
                Screen.width,
                Mathf.Max(0f, top)
            ),
            whiteTexture
        );

        GUI.DrawTexture(
            new Rect(
                0f,
                Screen.height -
                    Mathf.Max(0f, bottom),
                Screen.width,
                Mathf.Max(0f, bottom)
            ),
            whiteTexture
        );

        GUI.DrawTexture(
            new Rect(
                0f,
                top,
                Mathf.Max(0f, left),
                rect.height
            ),
            whiteTexture
        );

        GUI.DrawTexture(
            new Rect(
                Screen.width -
                    Mathf.Max(0f, right),
                top,
                Mathf.Max(0f, right),
                rect.height
            ),
            whiteTexture
        );

        GUI.color =
            Color.white;
    }

    // ============================================================
    // RETICLE
    // ============================================================

    void DrawReticle()
    {
        float cx =
            Screen.width * 0.5f;

        float cy =
            Screen.height * 0.5f;

        Color reticleColor =
            new Color(
                0.45f,
                1f,
                0.5f,
                0.9f
            );

        GUI.color =
            reticleColor;

        // horizontal
        GUI.DrawTexture(
            new Rect(
                cx - 90f,
                cy - reticleThickness * 0.5f,
                70f,
                reticleThickness
            ),
            whiteTexture
        );

        GUI.DrawTexture(
            new Rect(
                cx + 20f,
                cy - reticleThickness * 0.5f,
                70f,
                reticleThickness
            ),
            whiteTexture
        );

        // vertical
        GUI.DrawTexture(
            new Rect(
                cx - reticleThickness * 0.5f,
                cy - 90f,
                reticleThickness,
                70f
            ),
            whiteTexture
        );

        GUI.DrawTexture(
            new Rect(
                cx - reticleThickness * 0.5f,
                cy + 20f,
                reticleThickness,
                70f
            ),
            whiteTexture
        );

        // center dot
        GUI.DrawTexture(
            new Rect(
                cx - 3f,
                cy - 3f,
                6f,
                6f
            ),
            whiteTexture
        );

        // range marks
        for (int i = 1; i <= 4; i++)
        {
            float offset =
                i * 22f;

            GUI.DrawTexture(
                new Rect(
                    cx - 8f,
                    cy + offset,
                    16f,
                    1f
                ),
                whiteTexture
            );

            GUI.DrawTexture(
                new Rect(
                    cx - 8f,
                    cy - offset,
                    16f,
                    1f
                ),
                whiteTexture
            );
        }

        GUI.color =
            Color.white;
    }

    // ============================================================
    // INFORMATION
    // ============================================================

    void DrawScopeInfo()
    {
        GUI.Label(
            new Rect(
                0f,
                Screen.height * 0.17f,
                Screen.width,
                30f
            ),
            "2.5x  OPTIC",
            textStyle
        );

        GUI.Label(
            new Rect(
                0f,
                Screen.height * 0.82f,
                Screen.width,
                25f
            ),
            "OPTIC ACTIVE",
            smallStyle
        );
    }

    void OnDestroy()
    {
        if (maskTexture != null)
            Destroy(maskTexture);

        if (whiteTexture != null)
            Destroy(whiteTexture);
    }
}
