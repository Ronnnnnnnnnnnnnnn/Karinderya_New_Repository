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

            if (currentCustomers < maxCustomers)
            {
                SpawnCustomer();
            }
        }
    }

    // =========================================================
    // SPAWN CUSTOMER
    // =========================================================

    void SpawnCustomer()
    {
        // -----------------------------------------------------
        // BASIC CHECKS
        // -----------------------------------------------------

        if (customerPrefab == null)
        {
            Debug.LogError(
                "[SPAWNER] Customer Prefab is missing!"
            );

            return;
        }

        if (spawnPoint == null)
        {
            Debug.LogError(
                "[SPAWNER] Spawn Point is missing!"
            );

            return;
        }

        if (counterPoint == null)
        {
            Debug.LogError(
                "[SPAWNER] Counter Point is missing!"
            );

            return;
        }

        // -----------------------------------------------------
        // FIND UNLOCKED ORDERS
        // -----------------------------------------------------

        List<ItemData> unlockedOrders =
            GetUnlockedOrders();

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

            return;
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

        if (order != null)
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
            FindObjectOfType<RecipeBookCatalog>();

        if (catalog == null)
        {
            Debug.LogWarning(
                "[SPAWNER] RecipeBookCatalog not found. " +
                "Allowing dish: " +
                dish.itemName
            );

            return true;
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

            if (recipe.resultDish == dish)
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
}