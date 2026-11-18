using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerSpawner : MonoBehaviour
{
    [Header("Customer")]
    public GameObject customerPrefab;

    [Header("Points")]
    public Transform spawnPoint;
    public Transform counterPoint;
    public Transform exitPoint;
    public Transform lookPoint;

    [Header("Orders")]
    public ItemData[] possibleOrders;

    [Header("Spawn Settings")]
    public float spawnDelay = 10f;

    [Tooltip("Maximum number of customers allowed at the same time.")]
    public int maxCustomers = 2;

    // Runtime only. The tutorial pauses automatic spawning while it
    // controls the first customer; normal play leaves this true.
    bool spawningEnabled = true;

    int currentCustomers = 0;

    // =========================================================
    // START
    // =========================================================

    void Start()
    {
        Debug.Log("[SPAWNER] Started");

        StartCoroutine(SpawnLoop());
    }

    // =========================================================
    // SPAWN LOOP
    // =========================================================

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            float delay = spawnDelay;

            if (PlayerProgression.Instance != null)
            {
                delay =
                    PlayerProgression.Instance.GetSpawnDelay();
            }

            yield return new WaitForSeconds(delay);

            if (!spawningEnabled)
                continue;

            if (currentCustomers > 0 && !AnyCustomerAlive())
            {
                Debug.LogWarning(
                    "[SPAWNER] Customer count was stale. Resetting."
                );

                currentCustomers = 0;
            }

            if (currentCustomers < maxCustomers)
            {
                SpawnCustomer();
            }
        }
    }

    // =========================================================
    // SPAWN CUSTOMER
    // =========================================================

    CustomerOrder SpawnCustomer(ItemData forcedOrder = null)
    {
        // -----------------------------------------------------
        // BASIC CHECKS
        // -----------------------------------------------------

        if (customerPrefab == null)
        {
            Debug.LogError(
                "[SPAWNER] Customer Prefab is missing!"
            );

            return null;
        }

        if (spawnPoint == null)
        {
            Debug.LogError(
                "[SPAWNER] Spawn Point is missing!"
            );

            return null;
        }

        if (counterPoint == null)
        {
            Debug.LogError(
                "[SPAWNER] Counter Point is missing!"
            );

            return null;
        }

        // -----------------------------------------------------
        // FIND UNLOCKED ORDERS
        // -----------------------------------------------------

        List<ItemData> unlockedOrders = null;

        if (forcedOrder == null)
        {
            unlockedOrders = GetUnlockedOrders();

            // If unlock system isn't working,
            // still allow the first two dishes.
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

        // -----------------------------------------------------
        // INCREASE CUSTOMER COUNT
        // -----------------------------------------------------

        currentCustomers++;

        Debug.Log(
            "[SPAWNER] Spawning Customer " +
            currentCustomers +
            "/" +
            maxCustomers
        );

        // -----------------------------------------------------
        // CREATE CUSTOMER
        // -----------------------------------------------------

        GameObject customerObj =
            Instantiate(
                customerPrefab,
                spawnPoint.position,
                Quaternion.identity
            );

        // -----------------------------------------------------
        // CUSTOMER AI
        // -----------------------------------------------------

        CustomerAI ai =
            customerObj.GetComponent<CustomerAI>();

        if (ai != null)
        {
            ai.counterPoint =
                counterPoint;

            ai.exitPoint =
                exitPoint;

            ai.lookPoint =
                lookPoint;

            ai.spawner =
                this;

            Debug.Log(
                "[SPAWNER] Counter assigned"
            );
        }

        // -----------------------------------------------------
        // CUSTOMER ORDER
        // -----------------------------------------------------

        CustomerOrder order =
            customerObj.GetComponent<CustomerOrder>();

        if (order == null)
        {
            return null;
        }

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
                    Random.Range(
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

        return order;
    }

    // =========================================================
    // TUTORIAL SUPPORT
    // =========================================================

    public void SetSpawningEnabled(bool enabled)
    {
        spawningEnabled = enabled;
    }

    public CustomerOrder ForceSpawn(ItemData forcedOrder)
    {
        if (currentCustomers >= maxCustomers)
            return null;

        return SpawnCustomer(forcedOrder);
    }

    // =========================================================
    // GET UNLOCKED ORDERS
    // =========================================================

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

    // =========================================================
    // CHECK DISH UNLOCK
    // =========================================================

    bool IsDishUnlocked(ItemData dish)
    {
        if (dish == null)
            return false;

        // -----------------------------------------------------
        // FIND RECIPE CATALOG
        // -----------------------------------------------------

        RecipeBookCatalog catalog =
            RecipeBookCatalog.Find();

        // Without a catalog we can't tell what is unlocked, so don't
        // allow everything: the caller falls back to the default dishes.
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

        // -----------------------------------------------------
        // FIND RECIPE FOR THIS DISH
        // -----------------------------------------------------

        foreach (RecipeData recipe in catalog.recipes)
        {
            if (recipe == null)
                continue;

            // possibleOrders hold Serving items, recipes produce Dish items
            if (recipe.resultDish == dish ||
                (recipe.resultDish != null &&
                 recipe.resultDish.servingVersion == dish))
            {
                // No unlock manager
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

    // =========================================================
    // DEFAULT ORDERS
    // =========================================================

    List<ItemData> GetDefaultOrders()
    {
        List<ItemData> defaults =
            new List<ItemData>();

        if (possibleOrders == null)
            return defaults;

        // FIRST TWO DISHES ARE ALWAYS AVAILABLE
        // Kaldereta
        // Giniling

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

    // =========================================================
    // CUSTOMER LEFT
    // =========================================================

  public void CustomerLeft()
{
    currentCustomers--;

    if (currentCustomers < 0)
        currentCustomers = 0;

    Debug.Log(
        "[SPAWNER] Customer left. Current: " +
        currentCustomers +
        "/" +
        maxCustomers
    );
}

    // =========================================================
    // GET CURRENT CUSTOMERS
    // =========================================================

    public int GetCurrentCustomers()
    {
        return currentCustomers;
    }

    static bool AnyCustomerAlive()
    {
        CustomerAI[] customers =
            UnityEngine.Object.FindObjectsByType<CustomerAI>(
                FindObjectsSortMode.None
            );

        return customers.Length > 0;
    }
}