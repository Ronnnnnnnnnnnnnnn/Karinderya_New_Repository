using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerSpawner : MonoBehaviour
{
    [Serializable]
    public class ServiceSpot
    {
        [Tooltip("Where the customer stands to order.")]
        public Transform counterPoint;

        [Tooltip("Where the customer looks while waiting. Usually a child of the counter point.")]
        public Transform lookPoint;

        [NonSerialized]
        public CustomerAI occupant;
    }

    [Header("Customer")]
    public GameObject customerPrefab;

    [Header("Points")]
    public Transform spawnPoint;
    public Transform exitPoint;

    [Tooltip("Optional entry points around the front of the karinderya. " +
             "Customers spawn at a random one and walk back to it after eating.")]
    public Transform[] customerEntrances;

    [Header("Service Spots")]
    [Tooltip("One entry per counter where a customer can order.")]
    public ServiceSpot[] serviceSpots;

    [Header("Orders")]
    public ItemData[] possibleOrders;

    [Header("Spawn Settings")]
    [Tooltip("Delay before the first customer arrives.")]
    public float initialSpawnDelay = 5f;

    [Tooltip("Hard cap on simultaneous customers. Also limited by the number of service spots.")]
    [Min(1)]
    public int maxCustomers = 3;

    [Tooltip("Small random offset so two customers never start on the exact same spot.")]
    public float spawnSpread = 0.35f;

    [Header("Day Phases")]
    public bool useTimeOfDay = true;

    [Tooltip("Lunch rush starts at this hour (24-hour clock).")]
    public int lunchStartHour = 11;

    [Tooltip("Lunch rush ends at this hour.")]
    public int lunchEndHour = 14;

    [Tooltip("Dinner rush starts at this hour.")]
    public int dinnerStartHour = 17;

    [Tooltip("Dinner rush ends at this hour.")]
    public int dinnerEndHour = 21;

    [Tooltip("Low-traffic hours start here and wrap past midnight.")]
    public int lateNightStartHour = 22;

    [Tooltip("Low-traffic hours end here.")]
    public int lateNightEndHour = 6;

    [Header("Customers Per Phase")]
    [Tooltip("Most customers during lunch. The lunch rush usually fills both counters.")]
    public int lunchMaxCustomers = 2;

    [Tooltip("Most customers during dinner. The wide front can hold three.")]
    public int dinnerMaxCustomers = 3;

    [Tooltip("Most customers outside rush hours.")]
    public int normalMaxCustomers = 2;

    [Tooltip("Most customers late at night.")]
    public int lateNightMaxCustomers = 1;

    [Header("Arrival Timing")]
    [Tooltip("Seconds between arrival checks outside rush hours.")]
    public float normalDelayMin = 9f;
    public float normalDelayMax = 15f;

    [Tooltip("Seconds between arrival checks during lunch and dinner.")]
    public float rushDelayMin = 3.5f;
    public float rushDelayMax = 6.5f;

    [Tooltip("Seconds between arrival checks late at night.")]
    public float lateDelayMin = 16f;
    public float lateDelayMax = 24f;

    [Range(0f, 1f)]
    [Tooltip("Chance that a phase reaches its full customer cap instead of one fewer.")]
    public float rushMaxChance = 0.8f;

    [Range(0f, 1f)]
    [Tooltip("Chance that a second customer joins outside a rush.")]
    public float offPeakSecondCustomerChance = 0.3f;

    [Header("Rush Rewards")]
    [Tooltip("Coin multiplier during lunch and dinner. Makes the crowded hours worth the stress.")]
    [Min(1f)]
    public float rushRewardMultiplier = 1.25f;

    bool spawningEnabled = true;

    readonly List<CustomerAI> activeCustomers =
        new List<CustomerAI>();

    enum ServicePhase
    {
        Normal,
        LunchRush,
        DinnerRush,
        LateNight
    }

    ServicePhase lastPhase;
    bool phaseInitialized;

    void Awake()
    {

        if (serviceSpots == null || serviceSpots.Length == 0)
        {
            serviceSpots = FindServiceSpotsFromScene();
        }

        if (serviceSpots == null || serviceSpots.Length == 0)
        {
            Debug.LogWarning(
                "[SPAWNER] No service spots configured. " +
                "Customers will not spawn."
            );
        }
    }

    void Start()
    {
        Debug.Log("[SPAWNER] Started");

        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        yield return new WaitForSeconds(initialSpawnDelay);

        while (true)
        {
            if (!spawningEnabled)
            {
                yield return new WaitForSeconds(0.5f);
                continue;
            }

            PruneCustomers();

            ServicePhase phase = GetCurrentPhase();
            AnnouncePhase(phase);

            int desired = GetDesiredConcurrentCustomers(phase);
            int occupied = CountOccupiedSpots();

            if (desired > occupied &&
                activeCustomers.Count < GetMaxConcurrentCustomers())
            {
                CustomerOrder spawned = SpawnCustomer();

                if (spawned != null)
                {

                    yield return new WaitForSeconds(
                        UnityEngine.Random.Range(0.9f, 1.8f)
                    );

                    continue;
                }
            }

            yield return new WaitForSeconds(
                GetCheckDelay(phase)
            );
        }
    }

    CustomerOrder SpawnCustomer(ItemData forcedOrder = null)
    {

        if (customerPrefab == null)
        {
            Debug.LogError(
                "[SPAWNER] Customer Prefab is missing!"
            );

            return null;
        }

        Transform entrance = GetRandomEntrance();

        if (entrance == null && spawnPoint == null)
        {
            Debug.LogError(
                "[SPAWNER] No customer entrance or spawn point is set!"
            );

            return null;
        }

        List<int> freeSpots = GetFreeSpotIndices();

        if (freeSpots.Count == 0)
        {
            return null;
        }

        List<ItemData> unlockedOrders = null;

        if (forcedOrder == null)
        {
            unlockedOrders = GetUnlockedOrders();

            if (unlockedOrders.Count == 0)
            {
                Debug.LogWarning(
                    "[SPAWNER] No unlocked orders found. " +
                    "Using first two dishes as default."
                );

                unlockedOrders =
                    GetDefaultOrders();
            }

            if (unlockedOrders.Count == 0)
            {
                Debug.LogError(
                    "[SPAWNER] No customer orders available!"
                );

                return null;
            }
        }

        int spotIndex =
            freeSpots[
                UnityEngine.Random.Range(0, freeSpots.Count)
            ];

        ServiceSpot spot = serviceSpots[spotIndex];

        Vector3 basePosition =
            entrance != null
                ? entrance.position
                : spawnPoint.position;

        Vector3 spawnPosition = basePosition;

        if (spawnSpread > 0f)
        {
            spawnPosition += new Vector3(
                UnityEngine.Random.Range(-spawnSpread, spawnSpread),
                0f,
                UnityEngine.Random.Range(-spawnSpread, spawnSpread)
            );
        }

        GameObject customerObj =
            Instantiate(
                customerPrefab,
                spawnPosition,
                Quaternion.identity
            );

        CustomerAI ai =
            customerObj.GetComponent<CustomerAI>();

        CustomerOrder order =
            customerObj.GetComponent<CustomerOrder>();

        if (ai == null || order == null)
        {
            Debug.LogError(
                "[SPAWNER] Customer prefab is missing " +
                "CustomerAI or CustomerOrder."
            );

            Destroy(customerObj);

            return null;
        }

        CustomerAppearance appearance =
            customerObj.GetComponent<CustomerAppearance>();

        if (appearance == null)
        {
            appearance =
                customerObj.AddComponent<CustomerAppearance>();
        }

        CustomerType customerType =
            CustomerAppearance.RollType();

        appearance.Apply(customerType);

        Debug.Log(
            "[SPAWNER] Customer type: " +
            customerType
        );

        Transform exitTarget =
            entrance != null
                ? entrance
                : exitPoint;

        if (exitTarget == null)
            exitTarget = spawnPoint;

        ai.counterPoint = spot.counterPoint;
        ai.lookPoint = spot.lookPoint;
        ai.exitPoint = exitTarget;
        ai.spawner = this;
        ai.serviceSpotIndex = spotIndex;

        spot.occupant = ai;
        activeCustomers.Add(ai);

        if (forcedOrder != null)
        {
            order.SetOrder(forcedOrder);

            Debug.Log(
                "[SPAWNER] Tutorial customer ordered: " +
                forcedOrder.itemName
            );
        }
        else
        {
            ItemData chosen =
                unlockedOrders[
                    UnityEngine.Random.Range(
                        0,
                        unlockedOrders.Count
                    )
                ];

            if (chosen != null)
            {
                order.SetOrder(chosen);

                Debug.Log(
                    "[SPAWNER] Customer ordered: " +
                    chosen.itemName
                );
            }
        }

        order.ConfigureLabel(spotIndex);

        if (forcedOrder == null &&
            IsRushPhase(GetCurrentPhase()))
        {
            order.rewardCoins = Mathf.RoundToInt(
                order.rewardCoins * rushRewardMultiplier
            );

            order.isRushCustomer = true;
        }

        if (forcedOrder == null &&
            MissionManager.Instance != null &&
            MissionManager.Instance.RegularsUnlocked &&
            UnityEngine.Random.value <
            MissionManager.Instance.regularChance)
        {
            order.rewardCoins = Mathf.RoundToInt(
                order.rewardCoins *
                MissionManager.Instance.regularTipMultiplier
            );

            order.patienceTime +=
                MissionManager.Instance.regularPatienceBonus;

            order.SetRegular();

            Debug.Log(
                "[SPAWNER] Regular customer arrived."
            );
        }

        Debug.Log(
            "[SPAWNER] Spawning Customer " +
            activeCustomers.Count +
            "/" +
            GetMaxConcurrentCustomers() +
            " at counter " +
            (spotIndex + 1) +
            (entrance != null
                ? " from " + entrance.name
                : "")
        );

        return order;
    }

    ServicePhase GetCurrentPhase()
    {
        if (!useTimeOfDay || TimeManager.Instance == null)
            return ServicePhase.Normal;

        GameTimestamp timestamp =
            TimeManager.Instance.GetGameTimestamp();

        if (timestamp == null)
            return ServicePhase.Normal;

        int hour = timestamp.hour;

        if (IsInHourRange(
                hour,
                lunchStartHour,
                lunchEndHour))
        {
            return ServicePhase.LunchRush;
        }

        if (IsInHourRange(
                hour,
                dinnerStartHour,
                dinnerEndHour))
        {
            return ServicePhase.DinnerRush;
        }

        if (IsInHourRange(
                hour,
                lateNightStartHour,
                lateNightEndHour))
        {
            return ServicePhase.LateNight;
        }

        return ServicePhase.Normal;
    }

    static bool IsInHourRange(
        int hour,
        int startHour,
        int endHour)
    {
        if (startHour == endHour)
            return false;

        if (startHour < endHour)
        {
            return hour >= startHour &&
                   hour < endHour;
        }

        return hour >= startHour ||
               hour < endHour;
    }

    int GetDesiredConcurrentCustomers(ServicePhase phase)
    {
        int cap = GetMaxConcurrentCustomers();
        int desired;

        switch (phase)
        {
            case ServicePhase.LunchRush:
                desired =
                    UnityEngine.Random.value <
                    rushMaxChance
                        ? lunchMaxCustomers
                        : Mathf.Max(1, lunchMaxCustomers - 1);
                break;

            case ServicePhase.DinnerRush:
                desired =
                    UnityEngine.Random.value <
                    rushMaxChance
                        ? dinnerMaxCustomers
                        : Mathf.Max(1, dinnerMaxCustomers - 1);
                break;

            case ServicePhase.LateNight:
                desired = lateNightMaxCustomers;
                break;

            default:
                desired =
                    UnityEngine.Random.value <
                    offPeakSecondCustomerChance
                        ? normalMaxCustomers
                        : 1;
                break;
        }

        return Mathf.Clamp(desired, 1, cap);
    }

    static bool IsRushPhase(ServicePhase phase)
    {
        return phase == ServicePhase.LunchRush ||
               phase == ServicePhase.DinnerRush;
    }

    float GetCheckDelay(ServicePhase phase)
    {
        float min;
        float max;

        switch (phase)
        {
            case ServicePhase.LunchRush:
            case ServicePhase.DinnerRush:
                min = rushDelayMin;
                max = rushDelayMax;
                break;

            case ServicePhase.LateNight:
                min = lateDelayMin;
                max = lateDelayMax;
                break;

            default:
                min = normalDelayMin;
                max = normalDelayMax;
                break;
        }

        float speed = 1f;

        if (PlayerProgression.Instance != null)
        {
            float baseDelay = Mathf.Max(
                0.01f,
                PlayerProgression.Instance.baseSpawnDelay
            );

            speed = Mathf.Clamp(
                PlayerProgression.Instance.GetSpawnDelay() /
                baseDelay,
                0.35f,
                1f
            );
        }

        return UnityEngine.Random.Range(min, max) * speed;
    }

    void AnnouncePhase(ServicePhase phase)
    {
        if (!phaseInitialized)
        {
            phaseInitialized = true;
            lastPhase = phase;

            if (useTimeOfDay &&
                phase == ServicePhase.Normal &&
                NotificationManager.Instance != null)
            {
                NotificationManager.Instance.ShowMessage(
                    "Lunch rush at " +
                    lunchStartHour +
                    ":00. Dinner rush at " +
                    dinnerStartHour +
                    ":00."
                );
            }

            return;
        }

        if (phase == lastPhase)
            return;

        ServicePhase previousPhase = lastPhase;
        lastPhase = phase;

        if (NotificationManager.Instance == null)
            return;

        int bonusPercent = Mathf.RoundToInt(
            Mathf.Max(0f, rushRewardMultiplier - 1f) * 100f
        );

        switch (phase)
        {
            case ServicePhase.LunchRush:
                NotificationManager.Instance.ShowMessage(
                    "Lunch rush! Up to " +
                    lunchMaxCustomers +
                    " customers. Rush tips +" +
                    bonusPercent +
                    "%."
                );
                break;

            case ServicePhase.DinnerRush:
                NotificationManager.Instance.ShowMessage(
                    "Dinner rush! Up to " +
                    dinnerMaxCustomers +
                    " customers. Rush tips +" +
                    bonusPercent +
                    "%."
                );
                break;

            case ServicePhase.Normal:
                if (IsRushPhase(previousPhase))
                {
                    NotificationManager.Instance.ShowMessage(
                        "The rush is over. Time to restock."
                    );
                }
                break;
        }
    }

    int GetMaxConcurrentCustomers()
    {
        int spots =
            serviceSpots != null
                ? serviceSpots.Length
                : 0;

        if (spots <= 0)
            return 0;

        return Mathf.Clamp(
            maxCustomers,
            1,
            spots
        );
    }

    List<int> GetFreeSpotIndices()
    {
        List<int> freeSpots = new List<int>();

        if (serviceSpots == null)
            return freeSpots;

        for (int i = 0; i < serviceSpots.Length; i++)
        {
            ServiceSpot spot = serviceSpots[i];

            if (spot == null ||
                spot.counterPoint == null)
            {
                continue;
            }

            if (spot.occupant == null)
            {
                freeSpots.Add(i);
            }
        }

        return freeSpots;
    }

    int CountOccupiedSpots()
    {
        if (serviceSpots == null)
            return 0;

        int count = 0;

        foreach (ServiceSpot spot in serviceSpots)
        {
            if (spot != null &&
                spot.occupant != null)
            {
                count++;
            }
        }

        return count;
    }

    Transform GetRandomEntrance()
    {
        if (customerEntrances == null ||
            customerEntrances.Length == 0)
        {
            return null;
        }

        int validCount = 0;

        foreach (Transform entrance in customerEntrances)
        {
            if (entrance != null)
                validCount++;
        }

        if (validCount == 0)
            return null;

        int pick =
            UnityEngine.Random.Range(0, validCount);

        int seen = 0;

        foreach (Transform entrance in customerEntrances)
        {
            if (entrance == null)
                continue;

            if (seen == pick)
                return entrance;

            seen++;
        }

        return null;
    }

    ServiceSpot[] FindServiceSpotsFromScene()
    {
        List<ServiceSpot> found =
            new List<ServiceSpot>();

        for (int i = 1; i <= 4; i++)
        {
            Transform counter =
                FindSceneTransform(
                    "Customer Counter" + i
                );

            if (counter == null)
                continue;

            Transform look =
                counter.Find("CounterLookPoint");

            found.Add(
                new ServiceSpot
                {
                    counterPoint = counter,
                    lookPoint = look
                }
            );
        }

        if (found.Count > 0)
        {
            Debug.Log(
                "[SPAWNER] Auto-found " +
                found.Count +
                " service spots."
            );
        }

        return found.ToArray();
    }

    static Transform FindSceneTransform(string name)
    {
        GameObject found =
            GameObject.Find(name);

        return found != null
            ? found.transform
            : null;
    }

    public void SetSpawningEnabled(bool enabled)
    {
        spawningEnabled = enabled;
    }

    public CustomerOrder ForceSpawn(ItemData forcedOrder)
    {
        PruneCustomers();

        if (activeCustomers.Count >=
            GetMaxConcurrentCustomers())
        {
            return null;
        }

        return SpawnCustomer(forcedOrder);
    }

    List<ItemData> GetUnlockedOrders()
    {
        List<ItemData> unlocked =
            new List<ItemData>();

        if (possibleOrders == null ||
            possibleOrders.Length == 0)
        {
            return unlocked;
        }

        foreach (ItemData item in possibleOrders)
        {
            if (item == null)
                continue;

            if (IsDishUnlocked(item))
            {
                unlocked.Add(item);
            }
        }

        return unlocked;
    }

    bool IsDishUnlocked(ItemData dish)
    {
        if (dish == null)
            return false;

        RecipeBookCatalog catalog =
            RecipeBookCatalog.Find();

        if (catalog == null)
        {
            Debug.LogWarning(
                "[SPAWNER] RecipeBookCatalog not found. " +
                "Cannot check unlock for: " +
                dish.itemName
            );

            return false;
        }

        if (catalog.recipes == null)
            return false;

        foreach (RecipeData recipe in catalog.recipes)
        {
            if (recipe == null)
                continue;

            if (recipe.resultDish == dish ||
                (recipe.resultDish != null &&
                 recipe.resultDish.servingVersion == dish))
            {

                if (RecipeUnlockManager.Instance == null)
                {
                    Debug.LogWarning(
                        "[SPAWNER] RecipeUnlockManager missing. " +
                        "Allowing: " +
                        recipe.recipeName
                    );

                    return true;
                }

                bool unlocked =
                    RecipeUnlockManager.Instance
                    .IsUnlocked(recipe);

                Debug.Log(
                    "[SPAWNER] " +
                    recipe.recipeName +
                    " unlocked = " +
                    unlocked
                );

                return unlocked;
            }
        }

        Debug.LogWarning(
            "[SPAWNER] Recipe not found for: " +
            dish.itemName
        );

        return false;
    }

    List<ItemData> GetDefaultOrders()
    {
        List<ItemData> defaults =
            new List<ItemData>();

        if (possibleOrders == null)
            return defaults;

        for (int i = 0;
             i < possibleOrders.Length && i < 2;
             i++)
        {
            if (possibleOrders[i] != null)
            {
                defaults.Add(
                    possibleOrders[i]
                );
            }
        }

        return defaults;
    }

    public void CustomerLeft(CustomerAI customer)
    {
        activeCustomers.Remove(customer);

        if (serviceSpots != null)
        {
            foreach (ServiceSpot spot in serviceSpots)
            {
                if (spot != null &&
                    spot.occupant == customer)
                {
                    spot.occupant = null;
                }
            }
        }

        Debug.Log(
            "[SPAWNER] Customer left. Current: " +
            activeCustomers.Count +
            "/" +
            GetMaxConcurrentCustomers()
        );
    }

    void PruneCustomers()
    {
        for (int i = activeCustomers.Count - 1;
             i >= 0;
             i--)
        {
            if (activeCustomers[i] == null)
            {
                activeCustomers.RemoveAt(i);
            }
        }

        if (serviceSpots == null)
            return;

        foreach (ServiceSpot spot in serviceSpots)
        {
            if (spot == null)
                continue;

            if (spot.occupant == null)
            {
                spot.occupant = null;
            }
        }
    }

    public int GetCurrentCustomers()
    {
        PruneCustomers();

        return activeCustomers.Count;
    }
}
