using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerSpawner : MonoBehaviour
{
    [Header("Customer")]
    public GameObject customerPrefab;

    public Transform spawnPoint;

    public Transform counterPoint;

    public Transform exitPoint;

    [Header("Orders")]
    public List<ItemData> possibleOrders;

    [Header("Spawn")]
    public float spawnDelay = 10f;

    bool customerExists;

    void Start()
    {
        StartCoroutine(SpawnRoutine());

        Debug.Log("[SPAWNER] Started");
    }

    IEnumerator SpawnRoutine()
    {
        while(true)
        {
            if(!customerExists)
            {
                SpawnCustomer();
            }

            yield return new WaitForSeconds(
                spawnDelay
            );
        }
    }

    void SpawnCustomer()
    {
        Debug.Log(
            "[SPAWNER] Spawning customer"
        );

        if(customerPrefab == null)
        {
            Debug.LogError(
                "[SPAWNER] Customer Prefab missing!"
            );

            return;
        }

        GameObject customer =
            Instantiate(
                customerPrefab,
                spawnPoint.position,
                spawnPoint.rotation
            );

        customerExists = true;

        StartCoroutine(
            WaitForCustomerDestroy(customer)
        );

        // =========================
        // AI
        // =========================

        CustomerAI ai =
            customer.GetComponent<CustomerAI>();

        if(ai == null)
        {
            Debug.LogError(
                "[SPAWNER] CustomerAI missing!"
            );

            return;
        }

        ai.counterPoint = counterPoint;

        ai.exitPoint = exitPoint;

        // =========================
        // ORDER
        // =========================

        CustomerOrder order =
            customer.GetComponent<CustomerOrder>();

        if(order == null)
        {
            Debug.LogError(
                "[SPAWNER] CustomerOrder missing!"
            );

            return;
        }

        if(possibleOrders.Count <= 0)
        {
            Debug.LogError(
                "[SPAWNER] No orders assigned!"
            );

            return;
        }

        ItemData randomOrder =
            possibleOrders[
                Random.Range(
                    0,
                    possibleOrders.Count
                )
            ];

        order.SetOrder(randomOrder);

        Debug.Log(
            "[SPAWNER] Order Assigned: " +
            randomOrder.itemName
        );
    }

    IEnumerator WaitForCustomerDestroy(
        GameObject customer
    )
    {
        while(customer != null)
        {
            yield return null;
        }

        customerExists = false;

        Debug.Log(
            "[SPAWNER] Customer slot free"
        );
    }
}