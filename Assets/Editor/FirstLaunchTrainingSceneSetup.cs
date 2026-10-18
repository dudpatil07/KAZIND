using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Builds the training scene the first time the project opens with the blank
/// SampleScene. The saved scene is then used on later launches.
/// </summary>
[InitializeOnLoad]
public static class FirstLaunchTrainingSceneSetup
{
    private const string DefaultScenePath = "Assets/Scenes/SampleScene.unity";

    static FirstLaunchTrainingSceneSetup()
    {
        EditorApplication.delayCall += BuildIfSceneIsEmpty;
    }

    private static void BuildIfSceneIsEmpty()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            return;

        Scene scene = SceneManager.GetActiveScene();
        if (scene.path != DefaultScenePath || GameObject.Find("TrainingGround") != null)
            return;

        Debug.Log("Building the training scene for the first time...");

        BuildTrainingGround.Build();
        UpgradeTrainingGroundVisuals.Upgrade();
        SetupTacticalVisuals.Setup();
        AddMountainsAndClouds.Build();

        // The map builder adds a camera for preview. The player setup creates
        // the gameplay camera, so remove the preview camera to avoid two.
        GameObject previewCamera = GameObject.Find("TrainingGround/Main Camera");
        if (previewCamera != null)
            Object.DestroyImmediate(previewCamera);

        SetupFPSMechanics.Setup();
        SetupDroneThreatSystem.Setup();
        SetupDroneAI.Setup();
        FixDroneVisibility.Fix();
        SetupScopeInterface.Setup();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();

        Debug.Log("Training scene built and saved. Press Play to enter the drone training ground.");
    }
}
