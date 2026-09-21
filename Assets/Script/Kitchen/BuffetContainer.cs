using TMPro;
using UnityEngine;

public class BuffetContainer : MonoBehaviour
{
    [Header("Storage")]
    public ItemData storedDish;

    public int servings;

    public int maxServings = 10;

    [Header("This Buffet's Recipe")]
    public RecipeData buffetRecipe;

    [Header("Visuals")]
    public SpriteRenderer dishRenderer;

    public TextMeshPro servingsText;

    [Header("Sprites")]
    public Sprite emptySprite;

    [Header("Lock Visual")]
    public GameObject lockObject;

    public TextMeshPro lockText;

    [Header("Unlock Visual")]
    public SpriteRenderer buffetRenderer;

    // =====================================================
    // START
    // =====================================================

    // Sprite shown while the buffet is empty, so the player knows
    // which dish belongs here
    Sprite hintSprite;

    void Start()
    {
        CacheHintSprite();

        UpdateVisuals();
        UpdateLockVisual();

        Debug.Log(
            "[BUFFET] Ready: " +
            GetBuffetName()
        );
    }

    // =====================================================
    // INTERACT
    // =====================================================

    public void Interact()
    {
        Debug.Log(
            "[BUFFET] Interacted: " +
            GetBuffetName()
        );

        // =========================================
        // LOCKED
        // =========================================

        if (IsLocked())
        {
            UnlockBuffet();
            return;
        }

        // =========================================
        // UNLOCKED
        // =========================================

        // Put dish inside buffet
        if (storedDish == null)
        {
            InsertDish();
            return;
        }

        // Take serving
        GiveServing();
    }

    // =====================================================
    // CHECK LOCK
    // =====================================================

    bool IsLocked()
    {
        if (buffetRecipe == null)
            return false;

        if (RecipeUnlockManager.Instance == null)
        {
            Debug.LogWarning(
                "[BUFFET] RecipeUnlockManager missing!"
            );

            return true;
        }

        return !RecipeUnlockManager.Instance.IsUnlocked(
            buffetRecipe
        );
    }

    // =====================================================
    // UNLOCK BUFFET
    // =====================================================

    void UnlockBuffet()
    {
        if (buffetRecipe == null)
        {
            Debug.LogWarning(
                "[BUFFET] No recipe assigned!"
            );

            return;
        }

        if (RecipeUnlockManager.Instance == null)
        {
            Debug.LogWarning(
                "[BUFFET] RecipeUnlockManager missing!"
            );

            return;
        }

        if (RecipeUnlockManager.Instance.IsUnlocked(
            buffetRecipe))
        {
            UpdateLockVisual();
            return;
        }

        Debug.Log(
            "[BUFFET] Attempting to unlock: " +
            buffetRecipe.recipeName
        );

        bool success =
            RecipeUnlockManager.Instance.UnlockRecipe(
                buffetRecipe
            );

        if (success)
        {
            UpdateLockVisual();

            NotificationManager.Instance.ShowMessage(
                buffetRecipe.recipeName +
                " Buffet Unlocked!"
            );
        }
    }

    // =====================================================
    // INSERT DISH
    // =====================================================

    void InsertDish()
    {
        if (!InventoryManager.Instance.SlotEquipped(
            InventorySlot.InventoryType.Item))
        {
            NotificationManager.Instance.ShowMessage(
                "Hold A Dish!"
            );

            return;
        }

        ItemData heldItem =
            InventoryManager.Instance.GetEquippedSlotItem(
                InventorySlot.InventoryType.Item
            );

        if (heldItem == null)
            return;

        // ONLY ALLOW DISHES
        if (!heldItem.isDish)
        {
            NotificationManager.Instance.ShowMessage(
                "Item Is Not A Dish!"
            );

            return;
        }

        // =========================================
        // CHECK CORRECT DISH
        // =========================================

        if (buffetRecipe != null &&
            buffetRecipe.resultDish != heldItem)
        {
            NotificationManager.Instance.ShowMessage(
                "Wrong Dish For This Buffet!"
            );

            Debug.Log(
                "[BUFFET] Wrong dish. Expected: " +
                buffetRecipe.resultDish.itemName
            );

            return;
        }

        storedDish = heldItem;

        servings = maxServings;

        InventoryManager.Instance.ConsumeItem(
            InventoryManager.Instance.GetEquippedSlot(
                InventorySlot.InventoryType.Item
            )
        );

        Debug.Log(
            "[BUFFET] Stored: " +
            storedDish.itemName
        );

        UpdateVisuals();
    }

    // =====================================================
    // GIVE SERVING
    // =====================================================

    void GiveServing()
    {
        if (storedDish == null)
            return;

        if (servings <= 0)
        {
            EmptyBuffet();
            return;
        }

        ItemData serving =
            storedDish.servingVersion != null
            ? storedDish.servingVersion
            : storedDish;

        // Don't use up a serving if the bag is full
        if (!InventoryManager.Instance.AddItem(serving))
            return;

        servings--;

        Debug.Log(
            "[BUFFET] Serving Given: " +
            storedDish.itemName
        );

        if (servings <= 0)
        {
            EmptyBuffet();
        }

        UpdateVisuals();
    }

    // =====================================================
    // EMPTY BUFFET
    // =====================================================

    void EmptyBuffet()
    {
        storedDish = null;

        servings = 0;

        Debug.Log(
            "[BUFFET] Empty"
        );

        UpdateVisuals();
    }

    // =====================================================
    // VISUALS
    // =====================================================

    void CacheHintSprite()
    {
        // 1) The sprite already set on the renderer in the scene
        if (dishRenderer != null &&
            dishRenderer.sprite != null)
        {
            hintSprite = dishRenderer.sprite;
        }
        // 2) The sprite of the dish this buffet takes
        else if (buffetRecipe != null &&
                 buffetRecipe.resultDish != null)
        {
            hintSprite = buffetRecipe.resultDish.itemSprite;
        }
        // 3) Last resort
        else
        {
            hintSprite = emptySprite;
        }
    }

    void UpdateVisuals()
    {
        // Always show "servings/max", e.g. 0/10 when empty
        if (servingsText != null)
        {
            servingsText.text =
                servings +
                "/" +
                maxServings;
        }

        if (dishRenderer != null)
        {
            // Keep showing the dish sprite when empty
            // so the player knows what to put in
            dishRenderer.sprite =
                storedDish == null
                ? hintSprite
                : storedDish.itemSprite;
        }
    }

    // =====================================================
    // LOCK VISUAL
    // =====================================================

    void UpdateLockVisual()
    {
        if (buffetRecipe == null)
        {
            if (lockObject != null)
                lockObject.SetActive(false);

            if (lockText != null)
                lockText.text = "";

            return;
        }

        bool locked = IsLocked();

        // LOCK ICON
        if (lockObject != null)
        {
            lockObject.SetActive(locked);
        }

        // LOCK TEXT
        if (lockText != null)
        {
            if (locked)
            {
                lockText.text =
                    "LOCKED\n₱" +
                    buffetRecipe.unlockCost;
            }
            else
            {
                lockText.text =
                    buffetRecipe.recipeName;
            }
        }

        Debug.Log(
            "[BUFFET] " +
            buffetRecipe.recipeName +
            " Locked: " +
            locked
        );
    }

    // =====================================================
    // NAME
    // =====================================================

    string GetBuffetName()
    {
        if (buffetRecipe != null)
            return buffetRecipe.recipeName;

        return "Unassigned Buffet";
    }
}