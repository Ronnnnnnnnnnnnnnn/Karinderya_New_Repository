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

        // Put dish inside buffet
        if(storedDish == null)
        {
            InsertDish();
            return;
        }

        // Take serving
        GiveServing();
    }

    // =====================================================
    // INSERT DISH
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

        // Optional: only allow dishes
        if(!heldItem.isDish)
        {
            Debug.Log("[BUFFET] Item is not a dish");
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
            "[BUFFET] Stored " +
            storedDish.itemName
        );

        UpdateVisuals();
    }

    // =====================================================
    // GIVE SERVING
    // =====================================================

    void GiveServing()
    {
        if(storedDish == null)
            return;

        if(servings <= 0)
        {
            EmptyBuffet();
            return;
        }

        if (storedDish.servingVersion != null)
        {
            InventoryManager.Instance.AddItem(storedDish.servingVersion);
        }
        else
        {
            InventoryManager.Instance.AddItem(storedDish);
        }

        servings--;

        Debug.Log(
            "[BUFFET] Serving Given: " +
            storedDish.itemName
        );

        if(servings <= 0)
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

        Debug.Log("[BUFFET] Empty");

        UpdateVisuals();
    }

    // =====================================================
    // VISUALS
    // =====================================================

    void UpdateVisuals()
    {
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