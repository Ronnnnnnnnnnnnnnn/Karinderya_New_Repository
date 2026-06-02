using UnityEngine;

public class GameplayBootstrap : MonoBehaviour
{
    void Awake()
    {
        EnsureManager<CookingUIManager>("CookingUIManager");
        EnsureManager<FarmTimerUIManager>("FarmTimerUIManager");
        EnsureManager<PlayerProgression>("PlayerProgression");
    }

    static void EnsureManager<T>(string objectName) where T : Component
    {
        if (FindObjectOfType<T>() != null)
            return;

        GameObject go = new GameObject(objectName);
        go.AddComponent<T>();
    }
}
