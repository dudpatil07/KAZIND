using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Makes a fresh checkout launch the saved simulator without relying on Library/UserSettings.
/// </summary>
[InitializeOnLoad]
public static class FirstLaunchTrainingSceneSetup
{
    public const string DefaultScenePath = "Assets/Scenes/SampleScene.unity";
    private const string SessionKey = "KAZIND.StartupSceneConfigured";

    static FirstLaunchTrainingSceneSetup()
    {
        if (!Application.isBatchMode)
            EditorApplication.delayCall += ConfigureOnOpen;
    }

    private static void ConfigureOnOpen()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            return;
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            EditorApplication.delayCall += ConfigureOnOpen;
            return;
        }
        if (SessionState.GetBool(SessionKey, false)) return;
        if (!ConfigurePlayScene()) return;
        OpenSavedSceneIfSafe();
        SessionState.SetBool(SessionKey, true);
    }

    public static bool ConfigurePlayScene()
    {
        SceneAsset scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(DefaultScenePath);
        if (scene == null)
        {
            Debug.LogError("KAZIND: saved simulator scene is missing. Download the complete repository again.");
            return false;
        }
        EditorSceneManager.playModeStartScene = scene;
        return true;
    }

    public static void OpenSavedSceneIfSafe()
    {
        // Do not close another scene or discard unsaved work on a developer's existing checkout.
        Scene current = SceneManager.GetActiveScene();
        if (SceneManager.sceneCount == 1 && string.IsNullOrEmpty(current.path) && !current.isDirty)
            EditorSceneManager.OpenScene(DefaultScenePath, OpenSceneMode.Single);
    }

    [MenuItem("KAZIND/Play Simulator")]
    public static void PlaySimulator()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || !ConfigurePlayScene()) return;
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        EditorApplication.ExecuteMenuItem("Window/General/Game");
        EditorApplication.EnterPlaymode();
    }
}
