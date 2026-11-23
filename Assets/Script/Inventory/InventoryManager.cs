using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }

        if (SaveManager.HasSave)
        {
            LoadFromSave(SaveManager.Data);
        }
    }

    public void WriteSave(SaveData data)
    {
        data.toolSlots = ExportSlots(toolSlots);
        data.itemSlots = ExportSlots(itemSlots);
        data.equippedTool = ExportSlot(equippedToolSlot);
        data.equippedItem = ExportSlot(equippedItemSlot);
    }

    public void LoadFromSave(SaveData data)
    {
        if (data == null)
            return;

        toolSlots = ImportSlots(data.toolSlots, toolSlots.Length);
        itemSlots = ImportSlots(data.itemSlots, itemSlots.Length);
        equippedToolSlot = ImportSlot(data.equippedTool);
        equippedItemSlot = ImportSlot(data.equippedItem);
    }

    static List<SlotSave> ExportSlots(ItemSlotData[] slots)
    {
        List<SlotSave> result = new List<SlotSave>();

        foreach (ItemSlotData slot in slots)
        {
            result.Add(ExportSlot(slot));
        }

        return result;
    }

    static SlotSave ExportSlot(ItemSlotData slot)
    {
        if (slot == null || slot.IsEmpty())
        {
            return new SlotSave
            {
                itemName = "",
                quantity = 0
            };
        }

        return new SlotSave
        {
            itemName = slot.itemData.name,
            quantity = slot.quantity
        };
    }

    ItemSlotData[] ImportSlots(
        List<SlotSave> saves,
        int length)
    {
        ItemSlotData[] result = new ItemSlotData[length];

        for (int i = 0; i < length; i++)
        {
            result[i] = saves != null && i < saves.Count
                ? ImportSlot(saves[i])
                : new ItemSlotData(null, 0);
        }

        return result;
    }

    ItemSlotData ImportSlot(SlotSave save)
    {
        if (save == null ||
            string.IsNullOrEmpty(save.itemName) ||
            itemIndex == null)
        {
            return new ItemSlotData(null, 0);
        }

        ItemData item =
            itemIndex.GetItemFromString(save.itemName);

        if (item == null)
            return new ItemSlotData(null, 0);

        return new ItemSlotData(item, save.quantity);
    }

    public ItemIndex itemIndex;

    [Header("Tools")]
    [SerializeField] private ItemSlotData[] toolSlots = new ItemSlotData[8];

    [SerializeField] private ItemSlotData equippedToolSlot = null;

    [Header("Items")]
    [SerializeField] private ItemSlotData[] itemSlots = new ItemSlotData[8];

    [SerializeField] private ItemSlotData equippedItemSlot = null;

    public Vector3 handItemScale = Vector3.one;

    public Transform handPoint;

    public void InventoryToHand(int slotIndex, InventorySlot.InventoryType inventoryType)
    {
        ItemSlotData handToEquip = equippedToolSlot;
        ItemSlotData[] inventoryToAlter = toolSlots;

        if (inventoryType == InventorySlot.InventoryType.Item)
        {
            handToEquip = equippedItemSlot;
            inventoryToAlter = itemSlots;
        }

        if (handToEquip.Stackable(inventoryToAlter[slotIndex]))
        {
            ItemSlotData slotToAlter = inventoryToAlter[slotIndex];

            handToEquip.AddQuantity(slotToAlter.quantity);

            slotToAlter.Empty();
        }
        else
        {
            ItemSlotData slotToEquip =
                new ItemSlotData(inventoryToAlter[slotIndex]);

            inventoryToAlter[slotIndex] =
                new ItemSlotData(handToEquip);

            if (inventoryType == InventorySlot.InventoryType.Item)
            {
                equippedItemSlot = slotToEquip;
            }
            else
            {
                equippedToolSlot = slotToEquip;
            }
        }

        if (inventoryType == InventorySlot.InventoryType.Item)
        {
            RenderHand();
        }

        UIManager.Instance.RenderInventory();
    }

    public bool HandToInventory(InventorySlot.InventoryType inventoryType)
    {
        ItemSlotData handSlot = equippedToolSlot;
        ItemSlotData[] inventoryToAlter = toolSlots;

        if (inventoryType == InventorySlot.InventoryType.Item)
        {
            handSlot = equippedItemSlot;
            inventoryToAlter = itemSlots;
        }

        if (handSlot == null || handSlot.IsEmpty())
            return true;

        if (inventoryToAlter == null)
            return false;

        bool moved = StackItemToInventory(handSlot, inventoryToAlter);

        if (!moved)
        {
            for (int i = 0; i < inventoryToAlter.Length; i++)
            {
                if (inventoryToAlter[i] != null &&
                    inventoryToAlter[i].IsEmpty())
                {
                    inventoryToAlter[i] =
                        new ItemSlotData(handSlot);

                    handSlot.Empty();
                    moved = true;
                    break;
                }
            }
        }

        if (inventoryType == InventorySlot.InventoryType.Item)
        {
            RenderHand();
        }

        UIManager.Instance.RenderInventory();

        return moved;
    }

    public bool StackItemToInventory(
        ItemSlotData itemSlot,
        ItemSlotData[] inventoryArray)
    {
        if (itemSlot == null || inventoryArray == null)
            return false;

        for (int i = 0; i < inventoryArray.Length; i++)
        {
            if (inventoryArray[i] != null &&
                inventoryArray[i].Stackable(itemSlot))
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
        if (handPoint == null)
            return;

        if (handPoint.childCount > 0)
        {
            Destroy(handPoint.GetChild(0).gameObject);
        }

        if (SlotEquipped(InventorySlot.InventoryType.Item))
        {
            ItemData itemData =
                GetEquippedSlotItem(
                    InventorySlot.InventoryType.Item
                );

            if (itemData == null || itemData.gameModel == null)
            {
                Debug.LogWarning(
                    "[INVENTORY] Item has no world model: " +
                    (itemData != null ? itemData.name : "null")
                );

                return;
            }

            GameObject item =
                Instantiate(
                    itemData.gameModel,
                    handPoint
                );

            item.transform.localPosition = Vector3.zero;
            item.transform.localRotation = Quaternion.identity;
            item.transform.localScale = itemData.handScale;
        }
    }

    #region Gets and Checks

    public ItemData GetEquippedSlotItem(
        InventorySlot.InventoryType inventoryType)
    {
        if (inventoryType == InventorySlot.InventoryType.Item)
        {
            return equippedItemSlot.itemData;
        }

        return equippedToolSlot.itemData;
    }

    public ItemSlotData GetEquippedSlot(
        InventorySlot.InventoryType inventoryType)
    {
        if (inventoryType == InventorySlot.InventoryType.Item)
        {
            return equippedItemSlot;
        }

        return equippedToolSlot;
    }

    public ItemSlotData[] GetInventorySlots(
        InventorySlot.InventoryType inventoryType)
    {
        if (inventoryType == InventorySlot.InventoryType.Item)
        {
            return itemSlots;
        }

        return toolSlots;
    }

    public bool SlotEquipped(
        InventorySlot.InventoryType inventoryType)
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

        if (equipment != null)
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
            equippedToolSlot =
                new ItemSlotData(item);
        }
        else
        {
            equippedItemSlot =
                new ItemSlotData(item);
        }
    }

    public void EquipHandSlot(ItemSlotData itemSlot)
    {
        ItemData item = itemSlot.itemData;

        if (IsTool(item))
        {
            equippedToolSlot =
                new ItemSlotData(itemSlot);
        }
        else
        {
            equippedItemSlot =
                new ItemSlotData(itemSlot);
        }
    }

    public void ConsumeItem(ItemSlotData itemSlot)
    {
        if (itemSlot.IsEmpty())
        {
            Debug.LogError(
                "There is nothing to consume!"
            );

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
        if (slot == null)
            return;

        if (slot.itemData != null &&
            slot.quantity == 0)
        {
            slot.quantity = 1;
        }
    }

    void ValidateInventorySlots(ItemSlotData[] array)
    {
        if (array == null)
            return;

        foreach (ItemSlotData slot in array)
        {
            ValidateInventorySlot(slot);
        }
    }

    #endregion

    public int CountItem(ItemData item)
    {
        if (item == null)
        {
            return 0;
        }

        int count = 0;

        if (equippedItemSlot != null &&
            !equippedItemSlot.IsEmpty() &&
            equippedItemSlot.itemData == item)
        {
            count += equippedItemSlot.quantity;
        }

        foreach (ItemSlotData slot in itemSlots)
        {
            if (!slot.IsEmpty() &&
                slot.itemData == item)
            {
                count += slot.quantity;
            }
        }

        return count;
    }

    public bool HasItems(ItemData[] items)
    {
        if (items == null)
        {
            return true;
        }

        Dictionary<ItemData, int> needed =
            new Dictionary<ItemData, int>();

        foreach (ItemData item in items)
        {
            if (item == null)
            {
                continue;
            }

            needed.TryGetValue(
                item,
                out int count
            );

            needed[item] = count + 1;
        }

        foreach (KeyValuePair<ItemData, int> entry in needed)
        {
            if (CountItem(entry.Key) < entry.Value)
            {
                return false;
            }
        }

        return true;
    }

    public void ConsumeItems(ItemData[] items)
    {
        if (items == null)
        {
            return;
        }

        foreach (ItemData item in items)
        {
            if (item == null)
            {
                continue;
            }

            ConsumeOneFromInventory(item);
        }

        RenderHand();

        if (UIManager.Instance != null)
        {
            UIManager.Instance.RenderInventory();
        }
    }

    void ConsumeOneFromInventory(ItemData item)
    {
        if (equippedItemSlot != null &&
            !equippedItemSlot.IsEmpty() &&
            equippedItemSlot.itemData == item)
        {
            ConsumeItem(equippedItemSlot);
            return;
        }

        for (int i = 0; i < itemSlots.Length; i++)
        {
            if (!itemSlots[i].IsEmpty() &&
                itemSlots[i].itemData == item)
            {
                ConsumeItem(itemSlots[i]);
                return;
            }
        }
    }

    public bool HasRoomFor(ItemData item)
    {
        if (item == null)
        {
            return false;
        }

        ItemSlotData[] slots =
            IsTool(item)
                ? toolSlots
                : itemSlots;

        foreach (ItemSlotData slot in slots)
        {
            if (slot.IsEmpty() ||
                slot.itemData == item)
            {
                return true;
            }
        }

        return false;
    }

    public bool AddItem(ItemData item)
    {
        if (!HasRoomFor(item))
        {
            if (NotificationManager.Instance != null)
            {
                NotificationManager.Instance.ShowMessage(
                    "Inventory Full!"
                );
            }

            return false;
        }

        ItemSlotData newItem =
            new ItemSlotData(item);

        ItemSlotData[] slots =
            IsTool(item)
                ? toolSlots
                : itemSlots;

        if (!StackItemToInventory(newItem, slots))
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i].IsEmpty())
                {
                    slots[i] = newItem;
                    break;
                }
            }
        }

        UIManager.Instance.RenderInventory();

        return true;
    }

    void Start()
    {

    }

    void Update()
    {

    }
}
