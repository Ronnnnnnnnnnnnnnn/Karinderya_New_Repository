using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

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

    public ItemIndex itemIndex; 

    [Header("Tools")]
    [SerializeField] private ItemSlotData[] toolSlots = new ItemSlotData[8];
    
    [SerializeField]private ItemSlotData equippedToolSlot = null; 

    [Header("Items")]
    [SerializeField] private ItemSlotData[] itemSlots = new ItemSlotData[8];
    
    [SerializeField] private ItemSlotData equippedItemSlot = null;

    public Vector3 handItemScale = Vector3.one;

    public Transform handPoint; 

    public void InventoryToHand(int slotIndex, InventorySlot.InventoryType inventoryType)
    {
        ItemSlotData handToEquip = equippedToolSlot;
        ItemSlotData[] inventoryToAlter = toolSlots; 
        
        if(inventoryType == InventorySlot.InventoryType.Item)
        {
            handToEquip = equippedItemSlot;
            inventoryToAlter = itemSlots;
        }

        if (handToEquip.Stackable(inventoryToAlter[slotIndex]))
        {
            ItemSlotData slotToAlter = inventoryToAlter[slotIndex];

            handToEquip.AddQuantity(slotToAlter.quantity);

            slotToAlter.Empty();


        } else
        {
            ItemSlotData slotToEquip = new ItemSlotData(inventoryToAlter[slotIndex]);

            inventoryToAlter[slotIndex] = new ItemSlotData(handToEquip);

            EquipHandSlot(slotToEquip); 
        }

        if (inventoryType == InventorySlot.InventoryType.Item)
        {
            RenderHand();
        }

        UIManager.Instance.RenderInventory();

    }

    public void HandToInventory(InventorySlot.InventoryType inventoryType)
    {
        ItemSlotData handSlot = equippedToolSlot;
        ItemSlotData[] inventoryToAlter = toolSlots;

        if (inventoryType == InventorySlot.InventoryType.Item)
        {
            handSlot = equippedItemSlot;
            inventoryToAlter = itemSlots;
        }

        if (!StackItemToInventory(handSlot, inventoryToAlter))
        {
            for (int i = 0; i < inventoryToAlter.Length; i++)
            {
                if (inventoryToAlter[i].IsEmpty())
                {
                    inventoryToAlter[i] = new ItemSlotData(handSlot);
                    handSlot.Empty();
                    break;
                }
            }

        }

        if (inventoryType == InventorySlot.InventoryType.Item)
        {
            RenderHand();
        }

        UIManager.Instance.RenderInventory();

       
    }

    public bool StackItemToInventory(ItemSlotData itemSlot, ItemSlotData[] inventoryArray)
    {
        
        for (int i = 0; i < inventoryArray.Length; i++)
        {
            if (inventoryArray[i].Stackable(itemSlot))
            {
                inventoryArray[i].AddQuantity(itemSlot.quantity);
                itemSlot.Empty();
                return true; 
            }
        }

        return false; 
    }

    public void RenderHand()
    {
        if (handPoint.childCount > 0)
        {
            Destroy(handPoint.GetChild(0).gameObject);
        }

        if (SlotEquipped(InventorySlot.InventoryType.Item))
        {
            ItemData itemData = GetEquippedSlotItem(InventorySlot.InventoryType.Item);

            GameObject item = Instantiate(itemData.gameModel, handPoint);

            item.transform.localPosition = Vector3.zero;
            item.transform.localRotation = Quaternion.identity;
            item.transform.localScale = itemData.handScale;
        }
    }

    #region Gets and Checks
    public ItemData GetEquippedSlotItem(InventorySlot.InventoryType inventoryType)
    {
        if(inventoryType == InventorySlot.InventoryType.Item)
        {
            return equippedItemSlot.itemData;
        }
        return equippedToolSlot.itemData; 
    }

    public ItemSlotData GetEquippedSlot(InventorySlot.InventoryType inventoryType)
    {
        if (inventoryType == InventorySlot.InventoryType.Item)
        {
            return equippedItemSlot;
        }
        return equippedToolSlot;
    }

    public ItemSlotData[] GetInventorySlots(InventorySlot.InventoryType inventoryType)
    {
        if (inventoryType == InventorySlot.InventoryType.Item)
        {
            return itemSlots;
        }
        return toolSlots;
    }

    public bool SlotEquipped(InventorySlot.InventoryType inventoryType)
    {
        if (inventoryType == InventorySlot.InventoryType.Item)
        {
            return !equippedItemSlot.IsEmpty();
        }
        return !equippedToolSlot.IsEmpty();
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

    public void EquipHandSlot(ItemData item)
    {
        if (IsTool(item))
        {
            equippedToolSlot = new ItemSlotData(item); 
        } else
        {
            equippedItemSlot = new ItemSlotData(item); 
        }

    }

    public void EquipHandSlot(ItemSlotData itemSlot)
    {
        ItemData item = itemSlot.itemData;
        
        if (IsTool(item))
        {
            equippedToolSlot = new ItemSlotData(itemSlot);
        }
        else
        {
            equippedItemSlot = new ItemSlotData(itemSlot);
        }
    }

    public void ConsumeItem(ItemSlotData itemSlot)
    {
        if (itemSlot.IsEmpty())
        {
            Debug.LogError("There is nothing to consume!");
            return; 
        }

        itemSlot.Remove();
        RenderHand();
        UIManager.Instance.RenderInventory(); 
    }


    #region Inventory Slot Validation
    private void OnValidate()
    {
        ValidateInventorySlot(equippedToolSlot);
        ValidateInventorySlot(equippedItemSlot);

        ValidateInventorySlots(itemSlots);
        ValidateInventorySlots(toolSlots);

    }
    
    void ValidateInventorySlot(ItemSlotData slot)
    {
        if(slot.itemData != null && slot.quantity == 0)
        {
            slot.quantity = 1;
        }
    }

    void ValidateInventorySlots(ItemSlotData[] array)
    {
        foreach (ItemSlotData slot in array)
        {
            ValidateInventorySlot(slot);
        }
    }
    #endregion

    public int CountItem(ItemData item)
    {
        if (item == null)
            return 0;

        int count = 0;

        if (equippedItemSlot != null && !equippedItemSlot.IsEmpty() && equippedItemSlot.itemData == item)
            count += equippedItemSlot.quantity;

        foreach (ItemSlotData slot in itemSlots)
        {
            if (!slot.IsEmpty() && slot.itemData == item)
                count += slot.quantity;
        }

        return count;
    }

    public bool HasItems(ItemData[] items)
    {
        if (items == null)
            return true;

        foreach (ItemData item in items)
        {
            if (item == null)
                continue;

            if (CountItem(item) < 1)
                return false;
        }

        return true;
    }

    public void ConsumeItems(ItemData[] items)
    {
        if (items == null)
            return;

        foreach (ItemData item in items)
        {
            if (item == null)
                continue;

            ConsumeOneFromInventory(item);
        }

        RenderHand();
        if (UIManager.Instance != null)
            UIManager.Instance.RenderInventory();
    }

    void ConsumeOneFromInventory(ItemData item)
    {
        if (equippedItemSlot != null && !equippedItemSlot.IsEmpty() && equippedItemSlot.itemData == item)
        {
            ConsumeItem(equippedItemSlot);
            return;
        }

        for (int i = 0; i < itemSlots.Length; i++)
        {
            if (!itemSlots[i].IsEmpty() && itemSlots[i].itemData == item)
            {
                ConsumeItem(itemSlots[i]);
                return;
            }
        }
    }

    public void AddItem(ItemData item)
    {
        ItemSlotData newItem = new ItemSlotData(item);

        if(IsTool(item))
        {
            if(StackItemToInventory(newItem, toolSlots))
            {
                UIManager.Instance.RenderInventory();
                return;
            }

            for(int i = 0; i < toolSlots.Length; i++)
            {
                if(toolSlots[i].IsEmpty())
                {
                    toolSlots[i] = newItem;
                    break;
                }
            }
        }
        else
        {
            if(StackItemToInventory(newItem, itemSlots))
            {
                UIManager.Instance.RenderInventory();
                return;
            }

            for(int i = 0; i < itemSlots.Length; i++)
            {
                if(itemSlots[i].IsEmpty())
                {
                    itemSlots[i] = newItem;
                    break;
                }
            }
        }

        UIManager.Instance.RenderInventory();
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}