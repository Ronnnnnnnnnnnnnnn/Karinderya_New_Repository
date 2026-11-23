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

    Sprite hintSprite;

    void Start()
    {
        CacheHintSprite();

        if (SaveManager.HasSave &&
            buffetRecipe != null)
        {
            BuffetSlotSave saved =
                SaveManager.FindBuffetSave(
                    buffetRecipe.recipeName
                );

            if (saved != null)
                LoadFromSave(saved);
        }

        UpdateVisuals();
        UpdateLockVisual();

        Debug.Log("[BUFFET] Ready: " + GetBuffetName());
    }

    public BuffetSlotSave WriteSave()
    {
        return new BuffetSlotSave
        {
            recipeName = buffetRecipe != null
                ? buffetRecipe.recipeName
                : "",
            storedDishName = storedDish != null
                ? storedDish.name
                : "",
            servings = servings
        };
    }

    public void LoadFromSave(BuffetSlotSave saved)
    {
        if (saved == null)
            return;

        if (string.IsNullOrEmpty(saved.storedDishName) ||
            InventoryManager.Instance == null ||
            InventoryManager.Instance.itemIndex == null)
        {
            storedDish = null;
            servings = 0;
            return;
        }

        ItemData dish =
            InventoryManager.Instance.itemIndex.GetItemFromString(
                saved.storedDishName
            );

        storedDish = dish;
        servings = Mathf.Clamp(saved.servings, 0, maxServings);

        if (storedDish == null)
            servings = 0;
    }

    public void Interact()
    {
        Debug.Log("[BUFFET] Interacted: " + GetBuffetName());

        if (IsLocked())
        {
            UnlockBuffet();
            return;
        }

        if (storedDish == null)
        {
            InsertDish();
            return;
        }

        GiveServing();
    }

    bool IsLocked()
    {
        if (buffetRecipe == null)
        {
            return false;
        }

        if (RecipeUnlockManager.Instance == null)
        {
            Debug.LogWarning("[BUFFET] RecipeUnlockManager missing!");
            return true;
        }

        return !RecipeUnlockManager.Instance.IsUnlocked(buffetRecipe);
    }

    void UnlockBuffet()
    {
        if (buffetRecipe == null)
        {
            Debug.LogWarning("[BUFFET] No recipe assigned!");
            return;
        }

        if (RecipeUnlockManager.Instance == null)
        {
            Debug.LogWarning("[BUFFET] RecipeUnlockManager missing!");
            return;
        }

        if (RecipeUnlockManager.Instance.IsUnlocked(buffetRecipe))
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
        {
            return;
        }

        if (!heldItem.isDish)
        {
            NotificationManager.Instance.ShowMessage(
                "Item Is Not A Dish!"
            );

            return;
        }

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

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(
                AudioManager.Instance.serveSound
            );
        }
    }

    void GiveServing()
    {
        if (storedDish == null)
        {
            return;
        }

        if (servings <= 0)
        {
            EmptyBuffet();
            return;
        }

        ItemData serving =
            storedDish.servingVersion != null
                ? storedDish.servingVersion
                : storedDish;

        if (!InventoryManager.Instance.AddItem(serving))
        {
            return;
        }

        servings--;

        Debug.Log(
            "[BUFFET] Serving Given: " +
            storedDish.itemName
        );

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(
                AudioManager.Instance.dishTakeSound
            );
        }

        if (servings <= 0)
        {
            EmptyBuffet();
        }

        UpdateVisuals();
    }

    void EmptyBuffet()
    {
        storedDish = null;
        servings = 0;

        Debug.Log("[BUFFET] Empty");

        UpdateVisuals();
    }

    void CacheHintSprite()
    {
        if (dishRenderer != null &&
            dishRenderer.sprite != null)
        {
            hintSprite = dishRenderer.sprite;
        }
        else if (buffetRecipe != null &&
                 buffetRecipe.resultDish != null)
        {
            hintSprite =
                buffetRecipe.resultDish.itemSprite;
        }
        else
        {
            hintSprite = emptySprite;
        }
    }

    void UpdateVisuals()
    {
        if (servingsText != null)
        {
            servingsText.text =
                servings + "/" + maxServings;
        }

        if (dishRenderer != null)
        {
            bool empty = storedDish == null || servings <= 0;

            if (empty)
            {
                dishRenderer.sprite = emptySprite;
                dishRenderer.enabled = emptySprite != null;
            }
            else
            {
                dishRenderer.sprite = storedDish.itemSprite;
                dishRenderer.enabled = true;
            }
        }
    }

    public void RefreshLockVisual()
    {
        UpdateLockVisual();
    }

    void UpdateLockVisual()
    {
        if (buffetRecipe == null)
        {
            if (lockObject != null)
            {
                lockObject.SetActive(false);
            }

            if (lockText != null)
            {
                lockText.text = "";
            }

            return;
        }

        bool locked = IsLocked();

        if (lockObject != null)
        {
            lockObject.SetActive(locked);
        }

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

    string GetBuffetName()
    {
        if (buffetRecipe != null)
        {
            return buffetRecipe.recipeName;
        }

        return "Unassigned Buffet";
    }
}
