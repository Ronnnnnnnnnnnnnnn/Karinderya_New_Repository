using UnityEngine;

public class BuffetContainer : MonoBehaviour
{
    [Header("Stored Food")]
    public ItemData storedFood;

    [Header("Servings")]
    public int currentServings = 0;

    [Header("Visual")]
    public Transform foodPoint;

    [Header("Sprite Display")]
    public SpriteRenderer foodSpriteRenderer;

    private GameObject currentVisual;

    void Update()
    {
        // PUT FOOD INTO BUFFET
        if(Input.GetKeyDown(KeyCode.T))
        {
            AddFood();
        }
    }

    public void AddFood()
    {
        // BUFFET ALREADY HAS FOOD
        if(storedFood != null)
        {
            Debug.Log("Buffet already has food");
            return;
        }

        // GET HELD ITEM
        ItemData equipped =
            InventoryManager.Instance.GetEquippedSlotItem(
                InventorySlot.InventoryType.Item
            );

        // PLAYER HOLDING NOTHING
        if(equipped == null)
        {
            Debug.Log("Player holding nothing");
            return;
        }

        // STORE FOOD
        storedFood = equipped;

        // SERVINGS
        currentServings = equipped.servings;

        // REMOVE FROM INVENTORY
        InventoryManager.Instance.ConsumeItem(
            InventoryManager.Instance.GetEquippedSlot(
                InventorySlot.InventoryType.Item
            )
        );

        // UPDATE HAND
        InventoryManager.Instance.RenderHand();

        // SPAWN 3D MODEL
        SpawnVisual(storedFood.gameModel);

        // SHOW SPRITE ABOVE BUFFET
        if(foodSpriteRenderer != null)
        {
            foodSpriteRenderer.sprite =
                storedFood.itemSprite;
        }

        Debug.Log(
            storedFood.itemName +
            " added with " +
            currentServings +
            " servings"
        );
    }

    public bool TakeServing(ItemData wantedFood)
    {
        // EMPTY
        if(storedFood == null)
        {
            Debug.Log("Buffet empty");
            return false;
        }

        // WRONG FOOD
        if(storedFood != wantedFood)
        {
            Debug.Log("Wrong food");
            return false;
        }

        // REMOVE SERVING
        currentServings--;

        Debug.Log(
            "Served 1 portion. Remaining: " +
            currentServings
        );

        // EMPTY BUFFET
        if(currentServings <= 0)
        {
            EmptyBuffet();
        }

        return true;
    }

    void EmptyBuffet()
    {
        storedFood = null;

        currentServings = 0;

        // REMOVE MODEL
        if(currentVisual != null)
        {
            Destroy(currentVisual);
        }

        // REMOVE SPRITE
        if(foodSpriteRenderer != null)
        {
            foodSpriteRenderer.sprite = null;
        }

        Debug.Log("Buffet empty now");
    }

    void SpawnVisual(GameObject model)
    {
        if(model == null)
        {
            Debug.Log("No model assigned!");
            return;
        }

        if(foodPoint == null)
        {
            Debug.Log("Food Point missing!");
            return;
        }

        // DELETE OLD MODEL
        if(currentVisual != null)
        {
            Destroy(currentVisual);
        }

        // SPAWN MODEL
        currentVisual = Instantiate(
            model,
            foodPoint.position,
            foodPoint.rotation
        );

        currentVisual.transform.SetParent(foodPoint);

        currentVisual.transform.localPosition = Vector3.zero;

        currentVisual.transform.localRotation =
            Quaternion.identity;

        currentVisual.transform.localScale =
            Vector3.one;
    }
}