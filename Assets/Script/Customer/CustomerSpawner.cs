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

    [Header("Spawn")]
    public float spawnDelay = 10f;

    bool customerExists;

    // =====================================================
    // START
    // =====================================================

    void Start()
    {
        Debug.Log(
            "[SPAWNER] Started"
        );

        StartCoroutine(
            SpawnLoop()
        );
    }

    // =====================================================
    // SPAWN LOOP
    // =====================================================

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            float delay =
                spawnDelay;

            if (PlayerProgression.Instance != null)
            {
                delay =
                    PlayerProgression.Instance
                    .GetSpawnDelay();
            }

            yield return new WaitForSeconds(
                delay
            );

            if (!customerExists)
            {
                SpawnCustomer();
            }
        }
    }

    // =====================================================
    // SPAWN CUSTOMER
    // =====================================================

    void SpawnCustomer()
    {
        // Get only unlocked dishes
        List<ItemData> unlockedOrders =
            GetUnlockedOrders();

        // If nothing is available
        if (unlockedOrders.Count == 0)
        {
            Debug.Log(
                "[SPAWNER] No unlocked dishes available."
            );

            return;
        }

        customerExists = true;

        Debug.Log(
            "[SPAWNER] Spawning customer"
        );

        GameObject customerObj =
            Instantiate(
                customerPrefab,
                spawnPoint.position,
                Quaternion.identity
            );

        // =========================================
        // CUSTOMER AI
        // =========================================

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

            Transform npcModel =
                customerObj.transform.Find(
                    "npc"
                );

            if (npcModel != null)
            {
                ai.visualModel =
                    npcModel;
            }
        }

        // =========================================
        // CUSTOMER ORDER
        // =========================================

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
                order.SetOrder(
                    chosen
                );
            }
        }
    }

    // =====================================================
    // GET UNLOCKED ORDERS
    // =====================================================

    List<ItemData> GetUnlockedOrders()
    {
        List<ItemData> result =
            new List<ItemData>();

        if (possibleOrders == null)
            return result;

        foreach (ItemData dish in possibleOrders)
        {
            if (dish == null)
                continue;

            RecipeData recipe =
                FindRecipeForDish(dish);

            if (recipe == null)
                continue;

            if (RecipeUnlockManager.Instance != null &&
                RecipeUnlockManager.Instance.IsUnlocked(
                    recipe))
            {
                result.Add(dish);

                Debug.Log(
                    "[SPAWNER] Available Order: " +
                    dish.itemName
                );
            }
        }

        return result;
    }

    // =====================================================
    // FIND RECIPE
    // =====================================================

    RecipeData FindRecipeForDish(
        ItemData dish)
    {
        RecipeData[] recipes =
            Resources.FindObjectsOfTypeAll<RecipeData>();

        foreach (RecipeData recipe in recipes)
        {
            if (recipe == null)
                continue;

            if (recipe.resultDish == dish)
            {
                return recipe;
            }
        }

        return null;
    }

    // =====================================================
    // CUSTOMER LEFT
    // =====================================================

    public void CustomerLeft()
    {
        customerExists = false;

        Debug.Log(
            "[SPAWNER] Customer cleared"
        );
    }
}