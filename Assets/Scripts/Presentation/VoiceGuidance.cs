using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>Offline narration for navigation, controls and score feedback. No target selection.</summary>
[DisallowMultipleComponent]
public sealed class VoiceGuidance : MonoBehaviour
{
    public enum Context { Hidden, Menu, Gallery, Gameplay, Complete }
    public enum Cue { Welcome, Controls, Score, GoodLuck }
    private static readonly string[] Files = { "welcome", "controls", "score", "good_luck" };
    private static readonly string[] Captions =
    {
        "Welcome to the Drone Threat Simulator. Start by choosing a mission.",
        "In the field, move with W, A, S, and D, and aim with your mouse.",
        "Correct calls earn one hundred points.",
        "Good luck."
    };
    private readonly AudioClip[] clips = new AudioClip[4];
    private readonly Queue<Cue> pending = new Queue<Cue>();
    private AudioSource source;
    private Context context;
    private string subtitle = "";
    private float subtitleUntil;
    private float nextReminder;
    private float nextScoreCue;
    private GUIStyle captionStyle;
    public bool AllowReminders { get; set; }
    public bool Muted { get; private set; }
    public string CurrentSubtitle => subtitle;
    public int PendingCount => pending.Count;
    public bool ClipsReady => System.Array.TrueForAll(clips, clip => clip != null);

    private void Awake()
    {
        source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = false;
        source.spatialBlend = 0f;
        source.volume = 0.7f;
        source.priority = 64;
        source.ignoreListenerPause = true;
        for (int i = 0; i < clips.Length; i++)
        {
            clips[i] = Resources.Load<AudioClip>("Presentation/Voice/" + Files[i]);
            if (clips[i] == null) Debug.LogWarning("Voice clip missing: " + Files[i] + ". Subtitles remain available.", this);
        }
    }

    public void SetContext(Context value)
    {
        Clear();
        context = value;
        nextReminder = Time.unscaledTime + 60f;
        nextScoreCue = 0;
        if (value == Context.Menu) Play(Cue.Welcome);
        else if (value == Context.Gameplay)
        {
            Play(Cue.Controls);
            pending.Enqueue(Cue.GoodLuck);
        }
    }

    public void ReportProgress()
    {
        if (context != Context.Gameplay || Time.unscaledTime < nextScoreCue) return;
        // Never interrupt a spoken sentence, and never accumulate stale score announcements.
        if (pending.Count < 2 && !pending.Contains(Cue.Score)) pending.Enqueue(Cue.Score);
        nextScoreCue = Time.unscaledTime + 45f;
        nextReminder = Time.unscaledTime + 60f;
    }

    public void ReplayHelp()
    {
        if (context == Context.Hidden) return;
        Clear();
        Play(context == Context.Menu || context == Context.Gallery ? Cue.Welcome : Cue.Controls);
        nextReminder = Time.unscaledTime + 60f;
    }

    public void SetMuted(bool muted)
    {
        Muted = muted;
        source.mute = muted;
        // Keep the current subtitle and its timing when muting mid-sentence.
    }

    private void Play(Cue cue)
    {
        subtitle = Captions[(int)cue];
        AudioClip clip = clips[(int)cue];
        subtitleUntil = Time.unscaledTime + (clip != null ? clip.length : 5f) + 0.5f;
        source.clip = clip;
        if (clip != null) source.Play();
    }

    private void Update()
    {
        if (context == Context.Hidden) return;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.hKey.wasPressedThisFrame) ReplayHelp();
            if (Keyboard.current.vKey.wasPressedThisFrame) SetMuted(!Muted);
        }
        if (Time.unscaledTime < subtitleUntil) return;
        subtitle = "";
        if (pending.Count > 0) Play(pending.Dequeue());
        else if (context == Context.Gameplay && AllowReminders && !Muted && Time.unscaledTime >= nextReminder)
        {
            Play(Cue.Controls);
            nextReminder = Time.unscaledTime + 90f;
        }
    }

    private void Clear()
    {
        pending.Clear();
        if (source != null) source.Stop();
        subtitle = "";
        subtitleUntil = 0;
    }

    private void OnGUI()
    {
        if (context == Context.Hidden) return;
        int previousDepth = GUI.depth;
        GUI.depth = -1000;
        if (captionStyle == null)
            captionStyle = new GUIStyle(GUI.skin.label) { wordWrap = true, fontSize = 14,
                alignment = TextAnchor.MiddleLeft, normal = { textColor = Color.white } };
        float height = 50f;
        Rect footer = new Rect(0, Screen.height - height, Screen.width, height);
        Color old = GUI.color;
        GUI.color = new Color(0.025f, 0.04f, 0.06f, 0.94f);
        GUI.DrawTexture(footer, Texture2D.whiteTexture);
        GUI.color = old;
        string text = string.IsNullOrEmpty(subtitle) ? "HELP  H   •   VOICE  V   •   ESC  RELEASE MOUSE" : subtitle;
        GUI.Label(new Rect(16, footer.y + 3, Mathf.Max(80, Screen.width - 240), height - 6), text, captionStyle);
        if (GUI.Button(new Rect(Screen.width - 214, footer.y + 10, 96, 30), "HELP [H]")) ReplayHelp();
        if (GUI.Button(new Rect(Screen.width - 110, footer.y + 10, 100, 30), Muted ? "VOICE OFF [V]" : "VOICE ON [V]")) SetMuted(!Muted);
        GUI.depth = previousDepth;
    }

    private void OnDisable()
    {
        Clear();
        context = Context.Hidden;
    }

    private void OnDestroy()
    {
        if (source != null) Destroy(source);
    }
}
