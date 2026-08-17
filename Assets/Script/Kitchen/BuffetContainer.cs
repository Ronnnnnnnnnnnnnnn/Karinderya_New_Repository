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

    void Start()
    {
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

        if (storedDish.servingVersion != null)
        {
            InventoryManager.Instance.AddItem(
                storedDish.servingVersion
            );
        }
        else
        {
            InventoryManager.Instance.AddItem(
                storedDish
            );
        }

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

    void UpdateVisuals()
    {
        if (servingsText != null)
        {
            if (storedDish == null)
            {
                servingsText.text = "EMPTY";
            }
            else
            {
                servingsText.text =
                    servings +
                    "/" +
                    maxServings;
            }
        }

        if (dishRenderer != null)
        {
            if (storedDish == null)
            {
                dishRenderer.sprite =
                    emptySprite;
            }
            else
            {
                dishRenderer.sprite =
                    storedDish.itemSprite;
            }
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