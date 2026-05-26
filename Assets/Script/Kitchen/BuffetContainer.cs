using TMPro;
using UnityEngine;

public class BuffetContainer : MonoBehaviour
{
    [Header("Storage")]
    public ItemData storedDish;

    public int servings;

    public int maxServings = 10;

    [Header("Visuals")]
    public SpriteRenderer dishRenderer;

    public TextMeshPro servingsText;

    [Header("Sprites")]
    public Sprite emptySprite;

    void Start()
    {
        UpdateVisuals();

        Debug.Log("[BUFFET] Ready");
    }

    public void Interact()
    {
        Debug.Log("[BUFFET] Interacted");

        // INSERT DISH
        if(storedDish == null)
        {
            InsertDish();
            return;
        }

        // TAKE SERVING
        GiveServing();
    }

    // =====================================================
    // INSERT COOKED DISH
    // =====================================================

    void InsertDish()
    {
        if(!InventoryManager.Instance.SlotEquipped(
            InventorySlot.InventoryType.Item))
        {
            Debug.Log("[BUFFET] No item equipped");
            return;
        }

        ItemData heldItem =
            InventoryManager.Instance.GetEquippedSlotItem(
                InventorySlot.InventoryType.Item
            );

        if(heldItem == null)
            return;

        if(!heldItem.isDish)
        {
            Debug.Log("[BUFFET] Held item not dish");
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
        if(servings <= 0)
        {
            Debug.Log("[BUFFET] Empty");

            EmptyBuffet();

            return;
        }

        if(storedDish.servingVersion == null)
        {
            Debug.LogError(
                "[BUFFET] Serving Version Missing"
            );

            return;
        }

        InventoryManager.Instance.AddItem(
            storedDish.servingVersion
        );

        servings--;

        Debug.Log(
            "[BUFFET] Serving Given: " +
            storedDish.servingVersion.itemName
        );

        if(servings <= 0)
        {
            EmptyBuffet();
        }

        UpdateVisuals();
    }

    // =====================================================
    // EMPTY
    // =====================================================

    void EmptyBuffet()
    {
        Debug.Log("[BUFFET] Container Empty");

        storedDish = null;

        servings = 0;

        UpdateVisuals();
    }

    // =====================================================
    // VISUALS
    // =====================================================

    void UpdateVisuals()
    {
        // TEXT
        if(servingsText != null)
        {
            if(storedDish == null)
            {
                servingsText.text = "EMPTY";
            }
            else
            {
                servingsText.text =
                    servings + "/" + maxServings;
            }
        }

        // SPRITE
        if(dishRenderer != null)
        {
            if(storedDish == null)
            {
                dishRenderer.sprite = emptySprite;
            }
            else
            {
                dishRenderer.sprite =
                    storedDish.itemSprite;
            }
        }
    }
}