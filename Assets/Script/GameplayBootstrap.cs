using UnityEngine;
using UnityEngine.SceneManagement;

public class GameplayBootstrap : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Install()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;

        TryInstall(SceneManager.GetActiveScene());
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TryInstall(scene);
    }

    static void TryInstall(Scene scene)
    {
        if (!scene.IsValid() || !SceneHasCustomerSpawner(scene))
            return;

        GameplayBootstrap existing =
            Object.FindFirstObjectByType<GameplayBootstrap>();

        if (existing != null &&
            existing.gameObject.scene == scene)
        {
            return;
        }

        GameObject go = new GameObject("GameplayBootstrap");
        SceneManager.MoveGameObjectToScene(go, scene);
        go.AddComponent<GameplayBootstrap>();
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

    void Awake()
    {
        EnsureManager<CookingUIManager>(
            "CookingUIManager",
            gameObject.scene
        );

        EnsureManager<FarmTimerUIManager>(
            "FarmTimerUIManager",
            gameObject.scene
        );

        EnsureManager<PlayerProgression>(
            "PlayerProgression",
            gameObject.scene
        );
    }

    static void EnsureManager<T>(
        string objectName,
        Scene scene) where T : Component
    {
        T existing = Object.FindFirstObjectByType<T>();

        if (existing != null &&
            existing.gameObject.scene == scene)
        {
            return;
        }

        GameObject go = new GameObject(objectName);
        SceneManager.MoveGameObjectToScene(go, scene);
        go.AddComponent<T>();
    }
}
