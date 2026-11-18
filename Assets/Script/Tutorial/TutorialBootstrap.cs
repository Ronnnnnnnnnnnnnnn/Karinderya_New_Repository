using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Creates the tutorial after any scene containing a CustomerSpawner
/// is loaded. This keeps the tutorial out of the main menu and avoids
/// requiring a prefab or a manual scene edit.
/// </summary>
public static class TutorialBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Initialize()
    {
        // Avoid duplicate subscriptions when entering play mode with
        // domain reload disabled.
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;

        TryCreate(SceneManager.GetActiveScene());
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TryCreate(scene);
    }

    static void TryCreate(Scene scene)
    {
        if (!scene.IsValid())
            return;

        // Only the gameplay scene has a CustomerSpawner. Checking the
        // scene's own roots avoids creating a tutorial in the main menu
        // while the frozen game scene is still loaded behind it.
        if (!SceneHasCustomerSpawner(scene))
            return;

        TutorialManager existing =
            Object.FindFirstObjectByType<TutorialManager>();

        // If a tutorial already lives in this scene, do nothing.
        if (existing != null && existing.gameObject.scene == scene)
            return;

        GameObject tutorialObject = new GameObject("TutorialManager");
        SceneManager.MoveGameObjectToScene(tutorialObject, scene);
        tutorialObject.AddComponent<TutorialManager>();
    }

    static bool SceneHasCustomerSpawner(Scene scene)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root.GetComponentInChildren<CustomerSpawner>(true) != null)
                return true;
        }

        return false;
    }
}
