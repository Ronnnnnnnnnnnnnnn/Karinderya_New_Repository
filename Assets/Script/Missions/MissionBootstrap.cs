using UnityEngine;
using UnityEngine.SceneManagement;

public static class MissionBootstrap
{
    static readonly Vector3 BoardPosition =
        new Vector3(-6f, 0.05f, 10f);

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Initialize()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;

        TryCreate(SceneManager.GetActiveScene());
    }

    static void OnSceneLoaded(
        Scene scene,
        LoadSceneMode mode)
    {
        TryCreate(scene);
    }

    static void TryCreate(Scene scene)
    {
        if (!scene.IsValid() ||
            !SceneHasCustomerSpawner(scene))
        {
            return;
        }

        if (Object.FindFirstObjectByType<MissionManager>() == null)
        {
            GameObject managerObject =
                new GameObject("MissionManager");

            SceneManager.MoveGameObjectToScene(
                managerObject,
                scene
            );

            managerObject.AddComponent<MissionManager>();
        }

        if (Object.FindFirstObjectByType<MissionBoard>() == null)
        {
            GameObject boardObject =
                new GameObject("MissionBoard");

            SceneManager.MoveGameObjectToScene(
                boardObject,
                scene
            );

            boardObject.transform.position = BoardPosition;
            boardObject.transform.rotation =
                Quaternion.identity;

            boardObject.AddComponent<MissionBoard>();
        }
    }

    static bool SceneHasCustomerSpawner(Scene scene)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root.GetComponentInChildren<CustomerSpawner>(true) !=
                null)
            {
                return true;
            }
        }

        return false;
    }
}
