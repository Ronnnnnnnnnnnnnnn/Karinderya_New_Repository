using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager  : MonoBehaviour
{
    public static InventoryManager Instance {get; private set; }

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

    public void InventoryToHand(int slotIndex, InventorySlot.InventoryType inventoryType)
    {

        ItemSlotData handToEquip = equippedToolSlot;

        ItemSlotData[] inventoryToAlter = toolSlots;

        if(inventoryType == InventorySlot.InventoryType.Item)
        {
            handToEquip= equippedItemSlot;

            inventoryToAlter = itemSlots;
        }

        if(handToEquip.Stackable(inventoryToAlter[slotIndex]))
        {
            ItemSlotData slotToAlter = inventoryToAlter[slotIndex];

            handToEquip.AddQuantity(slotToAlter.quantity);

            slotToAlter.Empty();
        }else
        {
            ItemSlotData slotToEquip = new ItemSlotData(inventoryToAlter[slotIndex]);
        
            inventoryToAlter[slotIndex] = new ItemSlotData(handToEquip);

            EquipHandSlot(slotToEquip);
              
        }

        if(inventoryType == InventorySlot.InventoryType.Item)
        {
            RenderHand();
        }

        UIManager.Instance.RenderInventory();
    }

    public void HandToInventory(InventorySlot.InventoryType InventoryType)
    {
        ItemSlotData handSlot = equippedToolSlot;

        ItemSlotData[] inventoryToAlter = toolSlots;

        if(InventoryType == InventorySlot.InventoryType.Item)
        {
           handSlot = equippedItemSlot;

            inventoryToAlter = itemSlots;
        }

        if(!StackItemToInventory(handSlot, inventoryToAlter))
        {
            for(int i = 0; i < inventoryToAlter.Length; i++)
            {
                if(inventoryToAlter[i].isEmpty())
                {
                   inventoryToAlter[i] = new ItemSlotData(handSlot);

                    handSlot.Empty();

                    break;
                }
            }
        }

        if(InventoryType == InventorySlot.InventoryType.Item) 
            {
                RenderHand();
            }

        UIManager.Instance.RenderInventory();
    }

    public bool StackItemToInventory(ItemSlotData itemSlots, ItemSlotData[] inventoryArray)
    {
        for(int i = 0; i < inventoryArray.Length; i++)
        {
           if(inventoryArray[i].Stackable(itemSlots))
           {
            inventoryArray[i].AddQuantity(itemSlots.quantity);

            itemSlots.Empty();

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
            Instantiate(GetEquippedSlotItem(InventorySlot.InventoryType.Item).gameModel, handPoint);
        }
    }    

#region Get and Checks
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
            return !equippedItemSlot.isEmpty();
        }
        return !equippedToolSlot.isEmpty();
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

    public void  EquipHandSlot(ItemData item)
    {
        if(IsTool(item))
            {
                equippedToolSlot = new ItemSlotData(item);
            }else
            {
                equippedItemSlot = new ItemSlotData(item);
            } 
    }

    public void  EquipHandSlot(ItemSlotData itemSlots)
    {
        ItemData item = itemSlots.itemData;

        if(IsTool(item))
            {
                equippedToolSlot = new ItemSlotData(itemSlots);
            }else
            {
                equippedItemSlot = new ItemSlotData(itemSlots);
            } 
    }

    public void ConsumeItem(ItemSlotData itemSlots)
    {
        if(itemSlots.isEmpty())
        {
            Debug.LogError("There is no item to consume!");

            return;
        }
        itemSlots.Remove();

        RenderHand();

        UIManager.Instance.RenderInventory();
    }

#region Inventory Slot Validation
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
#endregion

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}