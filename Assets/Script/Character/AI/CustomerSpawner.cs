using UnityEngine;

public class CustomerSpawner : MonoBehaviour
{
    [Header("Customer Prefabs")]
    public GameObject[] customerPrefabs;

    [Header("Spawn Points")]
    public Transform[] spawnPoints;

    [Header("Counter Points")]
    public Transform[] counterPoints;

    [Header("Exit Point")]
    public Transform exitPoint;

    [Header("Orders")]
    public ItemData[] possibleOrders;

    [Header("Spawn Timing")]
    public float minSpawnTime = 10f;
    public float maxSpawnTime = 25f;

    private float timer;
    private float nextSpawnTime;

    void Start()
    {
        SetNextSpawnTime();
    }

    void Update()
    {
        timer += Time.deltaTime;

        if(timer >= nextSpawnTime)
        {
            SpawnCustomer();

            timer = 0f;

            SetNextSpawnTime();
        }
    }

    public void LeaveHappy()
    {

        Debug.Log("Customer leaving happy!");

        Destroy(gameObject, 3f);
    }

    void SetNextSpawnTime()
    {
        nextSpawnTime = Random.Range(
            minSpawnTime,
            maxSpawnTime
        );
    }

    void SpawnCustomer()
    {
        // RANDOM PREFAB
        GameObject prefab =
            customerPrefabs[
                Random.Range(0, customerPrefabs.Length)
            ];

        // RANDOM SPAWN
        Transform spawnPoint =
            spawnPoints[
                Random.Range(0, spawnPoints.Length)
            ];

        // RANDOM COUNTER
        Transform counterPoint =
            counterPoints[
                Random.Range(0, counterPoints.Length)
            ];

        // SPAWN CUSTOMER
        GameObject customer = Instantiate(
            prefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        // CUSTOMER AI
        CustomerAI ai =
            customer.GetComponentInChildren<CustomerAI>();

        ai.counterPoint = counterPoint;
        ai.exitPoint = exitPoint;

        // CUSTOMER ORDER
        CustomerOrder order =
            customer.GetComponentInChildren<CustomerOrder>();

        order.wantedItem =
            possibleOrders[
                Random.Range(0, possibleOrders.Length)
            ];

        Debug.Log(
            customer.name +
            " ordered: " +
            order.wantedItem.itemName
        );
    }
}