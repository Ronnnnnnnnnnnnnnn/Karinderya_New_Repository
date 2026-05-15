using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager  : MonoBehaviour
{
    public static InventoryManager Instance {get; private set; }
    public ItemData equippedItem;
    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }
    [Header("Tools")]

    [SerializeField]private ItemSlotData[] toolSlots = new ItemSlotData[8];

    [SerializeField]private ItemSlotData equippedToolSlot = null;

    [Header("Items")]
    
    [SerializeField]private ItemSlotData[] itemSlots = new ItemSlotData[8];

    [SerializeField]private ItemSlotData equippedItemSlot  = null;

    public Transform handPoint;

    public void InventoryToHand(int slotIndex, InventorySlot.InventoryType InventoryType)
    {
        if(InventoryType == InventorySlot.InventoryType.Item)
        {
            ItemSlotData itemToEquip = itemSlots[slotIndex];

            itemSlots[slotIndex] = equippedItemSlot;

            equippedItemSlot = itemToEquip;

            RenderHand();

        }else
        {
            ItemSlotData toolToEquip = toolSlots[slotIndex];

            toolSlots[slotIndex] = equippedToolSlot;

            equippedToolSlot = toolToEquip;
        }

        UIManager.Instance.RenderInventory();
    }

    public void HandToInventory(InventorySlot.InventoryType InventoryType)
    {
        /*
        if(InventoryType == InventorySlot.InventoryType.Item)
        {
            for(int i = 0; i < itemSlots.Length; i++)
            {
                if(itemSlots[i] == null)
                {
                    itemSlots[i] = equippedItemSlot;

                    equippedItemSlot = null;

                    break;
                }
            }

            RenderHand();

        }else
        {
             for(int i = 0; i < toolSlots.Length; i++)
            {
                if(toolSlots[i] == null)
                {
                    toolSlots[i] = equippedToolSlot;

                    equippedToolSlot = null;

                    break;
                }
            }
        }
        UIManager.Instance.RenderInventory();*/
    }

        public void RenderHand()
    {
        if (handPoint == null)
        {
            Debug.LogError("handPoint is NOT assigned!");
            return;
        }

        if (handPoint.childCount > 0)
        {
            Destroy(handPoint.GetChild(0).gameObject);
        }

        if (equippedItemSlot == null)
        {
            return;
        }

        if (equippedItemSlot.itemData.gameModel == null)
        {
            Debug.LogError("ItemSlotData gameModel is missing!");
            return;
        }

        GameObject obj = Instantiate(GetEquippedSlotItem(InventorySlot.InventoryType.Item).gameModel, handPoint);

        obj.transform.localPosition = new Vector3(0.3f, -0.3f, 0.7f);
        obj.transform.localRotation = Quaternion.Euler(0f, 90f, 90f);
        obj.transform.localScale = Vector3.one;
    }
#region Get and Checks
   public ItemData GetEquippedSlotItem(InventorySlot.InventoryType inventoryType)
{
    if (inventoryType == InventorySlot.InventoryType.Item)
    {
        if (equippedItemSlot == null)
        {
            return null;
        }

        return equippedItemSlot.itemData;
    }

    if (equippedToolSlot == null)
    {
        return null;
    }

    return equippedToolSlot.itemData;
}

    public ItemSlotData GetEquippedSlot(InventorySlot.InventoryType inventoryType)
    {
        if(inventoryType == InventorySlot.InventoryType.Item)
        {
            return equippedItemSlot;
        }
        return equippedToolSlot;
    }

    public ItemSlotData[] GetInventorySlots(InventorySlot.InventoryType inventoryType)
    {
        if(inventoryType == InventorySlot.InventoryType.Item)
        {
            return itemSlots;
        }
        return toolSlots;
    }

    public bool SlotEquipped(InventorySlot.InventoryType inventoryType)
    {
        if(inventoryType == InventorySlot.InventoryType.Item)
        {
            return equippedItemSlot != null;
        }
        return equippedToolSlot != null;
    }

    public bool IsTool(ItemData item)
    {
        EquipmentData equipment = item as EquipmentData;

        if(equipment != null)
        {
            return true;
        }

        SeedData seed = item as SeedData;

        return seed != null;
    }
#endregion

   public void EquipEmptySlot(ItemData item)
{
    if (IsTool(item))
    {
        equippedToolSlot = new ItemSlotData(item);
    }
    else
    {
        equippedItemSlot = new ItemSlotData(item);
    }

    RenderHand();

    UIManager.Instance.RenderInventory();
}

    private void OnValidate()
    {
        ValidateInventorySlot(equippedToolSlot);

        ValidateInventorySlot(equippedItemSlot);

        ValidateInventorySlot(itemSlots);

        ValidateInventorySlot(toolSlots);
    }

    void ValidateInventorySlot(ItemSlotData slot)
    {
        if(slot.itemData != null && slot.quantity == 0)
        {
            slot.quantity = 1;
        }
    }
     
    void ValidateInventorySlot(ItemSlotData[] array)
    {
        foreach (ItemSlotData slot in array)
        {
            ValidateInventorySlot(slot);
        }
    }
    void Start()
    {
        
    }

    void Update()
    {
        
    }
}