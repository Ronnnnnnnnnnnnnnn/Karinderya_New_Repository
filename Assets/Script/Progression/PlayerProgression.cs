using UnityEngine;

public class PlayerProgression : MonoBehaviour
{
    public static PlayerProgression Instance { get; private set; }

    [Header("Progress")]
    public int dishesServed;

    [Header("Spawn tuning")]
    public float baseSpawnDelay = 12f;
    public float minSpawnDelay = 4f;
    public float spawnDelayReductionPerLevel = 1.2f;

    public int Level => Mathf.FloorToInt(dishesServed / 3f);

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void OnDishServed(int coinsReward)
    {
        dishesServed++;
    }

    public float GetSpawnDelay()
    {
        float delay = baseSpawnDelay - Level * spawnDelayReductionPerLevel;
        return Mathf.Max(minSpawnDelay, delay);
    }

    public ItemData[] GetAvailableOrders(ItemData[] allOrders)
    {
        if (allOrders == null || allOrders.Length == 0)
            return allOrders;

        int maxOrders = Mathf.Clamp(1 + Level, 1, allOrders.Length);
        ItemData[] filtered = new ItemData[maxOrders];

        for (int i = 0; i < maxOrders; i++)
            filtered[i] = allOrders[i];

        return filtered;
    }

    public ItemData PickWeightedOrder(ItemData[] allOrders)
    {
        ItemData[] pool = GetAvailableOrders(allOrders);

        if (pool.Length == 0)
            return null;

        if (Level >= 2 && pool.Length > 1)
        {
            int roll = Random.Range(0, 100);
            if (roll < 35)
                return pool[pool.Length - 1];

            if (roll < 60 && pool.Length > 2)
                return pool[pool.Length - 2];
        }

        return pool[Random.Range(0, pool.Length)];
    }
}
