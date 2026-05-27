using System.Collections;
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

    void Start()
    {
        Debug.Log("[SPAWNER] Started");

        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        while(true)
        {
            yield return new WaitForSeconds(
                spawnDelay
            );

            // ONLY SPAWN IF NONE EXISTS
            if(!customerExists)
            {
                SpawnCustomer();
            }
        }
    }

    void SpawnCustomer()
    {
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

        // =====================================
        // CUSTOMER AI
        // =====================================

        CustomerAI ai =
            customerObj.GetComponent<CustomerAI>();

        if(ai != null)
        {
            ai.counterPoint = counterPoint;

            ai.exitPoint = exitPoint;

            ai.lookPoint = lookPoint;

            ai.spawner = this;

            Transform npcModel =
                customerObj.transform.Find("npc");

            if(npcModel != null)
            {
                ai.visualModel = npcModel;
            }
        }

        // =====================================
        // CUSTOMER ORDER
        // =====================================

        CustomerOrder order =
            customerObj.GetComponent<CustomerOrder>();

        if(order != null &&
            possibleOrders.Length > 0)
        {
            int randomIndex =
                Random.Range(
                    0,
                    possibleOrders.Length
                );

            order.SetOrder(
                possibleOrders[randomIndex]
            );
        }
    }

    // =========================================
    // CUSTOMER LEFT
    // =========================================

    public void CustomerLeft()
    {
        customerExists = false;

        Debug.Log(
            "[SPAWNER] Customer cleared"
        );
    }
}