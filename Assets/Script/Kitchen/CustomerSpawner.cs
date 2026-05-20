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

    void SetNextSpawnTime()
    {
        nextSpawnTime = Random.Range(
            minSpawnTime,
            maxSpawnTime
        );
    }

    void SpawnCustomer()
{
    // NO PREFABS
    if(customerPrefabs.Length <= 0)
    {
        Debug.LogError("No Customer Prefabs Assigned!");
        return;
    }

    // NO SPAWN POINTS
    if(spawnPoints.Length <= 0)
    {
        Debug.LogError("No Spawn Points Assigned!");
        return;
    }

    // NO COUNTER POINTS
    if(counterPoints.Length <= 0)
    {
        Debug.LogError("No Counter Points Assigned!");
        return;
    }

    // NO ORDERS
    if(possibleOrders.Length <= 0)
    {
        Debug.LogError("No Orders Assigned!");
        return;
    }

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

    // SPAWN
    GameObject customer = Instantiate(
        prefab,
        spawnPoint.position,
        spawnPoint.rotation
    );

    // GET AI
    CustomerAI ai =
        customer.GetComponent<CustomerAI>();

    if(ai == null)
    {
        Debug.LogError("CustomerAI missing!");
        return;
    }

    ai.counterPoint = counterPoint;

    ai.exitPoint = exitPoint;

    // GET ORDER SCRIPT
    CustomerOrder order =
        customer.GetComponent<CustomerOrder>();

    if(order == null)
    {
        Debug.LogError("CustomerOrder missing!");
        return;
    }

    // ASSIGN ORDER
    ItemData randomOrder =
        possibleOrders[
            Random.Range(0, possibleOrders.Length)
        ];

    order.wantedItem = randomOrder;

    ai.wantedFood = randomOrder;

    ai.UpdateOrderSprite();

    Debug.Log(
        customer.name +
        " ordered " +
        randomOrder.itemName
    );
}
}