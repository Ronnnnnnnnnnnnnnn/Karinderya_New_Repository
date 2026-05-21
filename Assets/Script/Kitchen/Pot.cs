using UnityEngine;

public class Pot : MonoBehaviour
{
    [Header("Current Food")]
    public ItemData currentItem;

    [Header("Cooking")]
    public float cookTime = 5f;
    public float burnTime = 10f;

    private float timer;

    private bool isCooking;
    private bool isCooked;

    [Header("Visual")]
    public Transform spawnPoint;

    [Header("Sprite Visuals")]
    public SpriteRenderer foodSpriteRenderer;

    public SpriteRenderer timerSpriteRenderer;

    public Sprite[] timerSprites;

    private GameObject currentVisual;

    void Update()
    {
        // INSERT INGREDIENT
        if(Input.GetKeyDown(KeyCode.R))
        {
            AddIngredient();
        }

        // TAKE FOOD
        if(Input.GetKeyDown(KeyCode.E))
        {
            TakeCookedFood();
        }

        // NOT COOKING
        if(!isCooking)
            return;

        timer += Time.deltaTime;

        UpdateTimerSprite();

        // FINISH COOKING
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
    // ADD INGREDIENT
    // =========================

    public void AddIngredient()
    {
        // POT OCCUPIED
        if(currentItem != null)
        {
            Debug.Log("Pot already has item!");
            return;
        }

        // GET HELD ITEM
        ItemData equipped =
            InventoryManager.Instance.GetEquippedSlotItem(
                InventorySlot.InventoryType.Item
            );

        // NOTHING HELD
        if(equipped == null)
        {
            Debug.Log("Player holding nothing");
            return;
        }

        // NOT COOKABLE
        if(!equipped.cookable)
        {
            Debug.Log("Cannot cook this item");
            return;
        }

        // STORE ITEM
        currentItem = equipped;

        // REMOVE FROM INVENTORY
        InventoryManager.Instance.ConsumeItem(
            InventoryManager.Instance.GetEquippedSlot(
                InventorySlot.InventoryType.Item
            )
        );

        // UPDATE HAND
        InventoryManager.Instance.RenderHand();

        // SHOW SPRITE
        if(foodSpriteRenderer != null)
        {
            foodSpriteRenderer.sprite =
                currentItem.itemSprite;
        }

        // SPAWN MODEL
        SpawnVisual(currentItem.gameModel);

        // START COOKING
        StartCooking();
    }

    // =========================
    // START COOKING
    // =========================

    void StartCooking()
    {
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

        isCooking = false;

        // NO COOKED VERSION
        if(currentItem.cookedVersion == null)
        {
            Debug.Log("No cooked version assigned!");
            return;
        }

        // REPLACE ITEM
        currentItem = currentItem.cookedVersion;

        // UPDATE SPRITE
        if(foodSpriteRenderer != null)
        {
            foodSpriteRenderer.sprite =
                currentItem.itemSprite;
        }

        // UPDATE MODEL
        ReplaceVisual(currentItem.gameModel);

        Debug.Log("Food cooked!");
    }

    // =========================
    // BURN FOOD
    // =========================

    void BurnFood()
    {
        isCooking = false;

        isCooked = false;

        // NO BURNT VERSION
        if(currentItem.burnedVersion == null)
        {
            Debug.Log("No burned version assigned!");
            return;
        }

        // REPLACE ITEM
        currentItem = currentItem.burnedVersion;

        // UPDATE SPRITE
        if(foodSpriteRenderer != null)
        {
            foodSpriteRenderer.sprite =
                currentItem.itemSprite;
        }

        // UPDATE MODEL
        ReplaceVisual(currentItem.gameModel);

        Debug.Log("Food burned!");
    }

    // =========================
    // TAKE FOOD
    // =========================

    public void TakeCookedFood()
    {
        if(currentItem == null)
        {
            Debug.Log("Pot empty");
            return;
        }

        // PUT FOOD IN PLAYER HAND
        InventoryManager.Instance.EquipHandSlot(currentItem);

        // UPDATE HAND MODEL
        InventoryManager.Instance.RenderHand();

        Debug.Log("Taken: " + currentItem.itemName);

        // RESET POT
        currentItem = null;

        isCooking = false;

        isCooked = false;

        timer = 0f;

        // CLEAR SPRITES
        if(foodSpriteRenderer != null)
        {
            foodSpriteRenderer.sprite = null;
        }

        if(timerSpriteRenderer != null)
        {
            timerSpriteRenderer.sprite = null;
        }

        // REMOVE MODEL
        if(currentVisual != null)
        {
            Destroy(currentVisual);
        }
    }

    // =========================
    // SPAWN MODEL
    // =========================

    void SpawnVisual(GameObject model)
    {
        if(model == null)
        {
            Debug.Log("No model assigned!");
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

        currentVisual.transform.localPosition =
            Vector3.zero;

        currentVisual.transform.localRotation =
            Quaternion.identity;

        currentVisual.transform.localScale =
            Vector3.one;
    }

    // =========================
    // REPLACE MODEL
    // =========================

    void ReplaceVisual(GameObject model)
    {
        if(currentVisual != null)
        {
            Destroy(currentVisual);
        }

        SpawnVisual(model);
    }

    // =========================
    // TIMER SPRITES
    // =========================

    void UpdateTimerSprite()
    {
        if(timerSpriteRenderer == null)
            return;

        if(timerSprites.Length <= 0)
            return;

        float progress =
            timer / cookTime;

        int index =
            Mathf.Clamp(
                Mathf.FloorToInt(
                    progress * timerSprites.Length
                ),
                0,
                timerSprites.Length - 1
            );

        timerSpriteRenderer.sprite =
            timerSprites[index];
    }
}