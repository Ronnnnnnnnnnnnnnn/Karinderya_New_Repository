using UnityEngine;

public class Pot : MonoBehaviour
{
    [Header("TEST FOOD")]
    public ItemData testRawFood;

    [Header("Current Food")]
    public ItemData currentItem;

    [Header("Cooking")]
    public float cookTime = 5f;
    public float burnTime = 10f;

    private float timer;
    private bool isCooking;
    private bool isCooked;

    [Header("Food Results")]
    public ItemData cookedItem;
    public ItemData burnedItem;

    [Header("Visuals")]
    public Transform spawnPoint;

    private GameObject currentVisual;

    void Update()
    {
        // PRESS R TO ADD RAW FOOD
        if(Input.GetKeyDown(KeyCode.R))
        {
            // ONLY ADD IF EMPTY
            if(currentItem == null)
            {
                AddItem(testRawFood);

                StartCooking();
            }
            else
            {
                Debug.Log("Pot already has item!");
            }
        }

        // STOP IF NOT COOKING
        if (!isCooking || currentItem == null)
            return;

        timer += Time.deltaTime;

        // COOK FOOD
        if(timer >= cookTime && !isCooked)
        {
            FinishCooking();
        }

        // BURN FOOD
        if(timer >= burnTime)
        {
            BurnFood();
        }
    }

    // =========================
    // ADD ITEM
    // =========================

    public void AddItem(ItemData item)
    {
        if(item == null)
        {
            Debug.Log("No item given to pot!");
            return;
        }

        if(currentItem != null)
        {
            Debug.Log("Pot already has item!");
            return;
        }

        currentItem = item;

        SpawnVisual(currentItem.gameModel);

        Debug.Log("Added: " + item.itemName);
    }

    // =========================
    // START COOKING
    // =========================

    public void StartCooking()
    {
        if(currentItem == null)
        {
            Debug.Log("No item to cook!");
            return;
        }

        isCooking = true;
        isCooked = false;
        timer = 0f;

        Debug.Log("Cooking started");
    }

    // =========================
    // FINISH COOKING
    // =========================

    void FinishCooking()
    {
        isCooked = true;

        currentItem = cookedItem;

        ReplaceVisual(cookedItem.gameModel);

        Debug.Log("Food cooked!");
    }

    // =========================
    // TAKE ITEM
    // =========================

    public ItemData TakeItem()
    {
        if(currentItem == null)
        {
            Debug.Log("Pot empty!");
            return null;
        }

        ItemData item = currentItem;

        currentItem = null;

        isCooking = false;
        isCooked = false;

        timer = 0f;

        if(currentVisual != null)
        {
            Destroy(currentVisual);
        }

        Debug.Log("Taken: " + item.itemName);

        return item;
    }

    // =========================
    // CHECKS
    // =========================

    public bool HasCookedFood()
    {
        return currentItem != null && isCooked;
    }

    public bool HasAnyItem()
    {
        return currentItem != null;
    }

    // =========================
    // BURN FOOD
    // =========================

    void BurnFood()
    {
        currentItem = burnedItem;

        ReplaceVisual(burnedItem.gameModel);

        isCooking = false;
        isCooked = false;

        timer = 0f;

        Debug.Log("Food burned!");
    }

    // =========================
    // SPAWN VISUAL
    // =========================

    void SpawnVisual(GameObject model)
    {
        if(model == null)
        {
            Debug.Log("Model missing!");
            return;
        }

        if(spawnPoint == null)
        {
            Debug.Log("Spawn Point missing!");
            return;
        }

        if(currentVisual != null)
        {
            Destroy(currentVisual);
        }

        currentVisual = Instantiate(
            model,
            spawnPoint.position,
            spawnPoint.rotation
        );

        currentVisual.transform.SetParent(spawnPoint);

        currentVisual.transform.localPosition = Vector3.zero;
        currentVisual.transform.localRotation = Quaternion.identity;
        currentVisual.transform.localScale = Vector3.one;
    }

    // =========================
    // REPLACE VISUAL
    // =========================

    void ReplaceVisual(GameObject model)
    {
        if(currentVisual != null)
        {
            Destroy(currentVisual);
        }

        SpawnVisual(model);
    }
}