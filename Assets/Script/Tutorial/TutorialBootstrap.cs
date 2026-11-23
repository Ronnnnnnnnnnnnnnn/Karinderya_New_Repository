using UnityEngine;
using UnityEngine.SceneManagement;

public static class TutorialBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Initialize()
    {

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

        if (!SceneHasCustomerSpawner(scene))
            return;

        TutorialManager existing =
            Object.FindFirstObjectByType<TutorialManager>();

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
