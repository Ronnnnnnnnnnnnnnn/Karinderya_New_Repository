using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour, ITimeTracker
{ 
    public static UIManager Instance {get; private set; }

    [Header("Status Bar")]

    public Image toolEquipSlot;

    public Text toolQuantityText;

    public Text timeText;

    public Text dateText;

    [Header("Inventory System")]

    public GameObject inventoryPanel;

    public HandInventorySlot toolHandSlot;

    public InventorySlot[] toolSlots;

    public HandInventorySlot itemHandSlot;

    public InventorySlot[] itemSlots;

    public Text itemNameText;

    public Text itemDescriptionText;

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

    private void Start()
    {
        RenderInventory();
        AssignSlotIndexes();

        TimeManager.Instance.RegisterTracker(this);
    }

    public void AssignSlotIndexes()
    {
        for(int i=0; i<toolSlots.Length; i++)
        {
            toolSlots[i].AssignIndex(i);
            itemSlots[i].AssignIndex(i);
        }
    }

    public void RenderInventory()
    {
        ItemSlotData[] inventoryToolSlots = InventoryManager.Instance.GetInventorySlots(InventorySlot.InventoryType.Tool);
        
        ItemSlotData[] inventoryItemSlots = InventoryManager.Instance.GetInventorySlots(InventorySlot.InventoryType.Item);
        
        RenderInventoryPanel(inventoryToolSlots, toolSlots);

        RenderInventoryPanel(inventoryItemSlots, itemSlots);

        toolHandSlot.Display(InventoryManager.Instance.GetEquippedSlot(InventorySlot.InventoryType.Tool));

        itemHandSlot.Display(InventoryManager.Instance.GetEquippedSlot(InventorySlot.InventoryType.Item));

        ItemData equippedTool = InventoryManager.Instance.GetEquippedSlotItem(InventorySlot.InventoryType.Tool);

        toolQuantityText.text = "";

        if (equippedTool!= null)
        {
            toolEquipSlot.sprite = equippedTool.thumbnail;

            toolEquipSlot.gameObject.SetActive(true);

            int quantity = InventoryManager.Instance.GetEquippedSlot(InventorySlot.InventoryType.Tool).quantity;

            if(quantity > 1)
            {
                toolQuantityText.text = quantity.ToString();
            }

            return;
        }

        toolEquipSlot.gameObject.SetActive(false);
    }
    
    void RenderInventoryPanel(ItemSlotData[] slots, InventorySlot[] uiSlots)
    {
        for(int i=0; i < uiSlots.Length; i++)
        {
            uiSlots[i].Display(slots[i]);
        }
    }

       public void ToggleInventoryPanel()
   {
        inventoryPanel.SetActive(!inventoryPanel.activeSelf);

        RenderInventory();
   }

   public void DisplayItemInfo(ItemData data)
   {
        if(data == null)
        {
            itemNameText.text = "";
            itemDescriptionText.text = "";

            return;
        }

    itemNameText.text = data.name;

    itemDescriptionText.text = data.description;
   }

   public void ClockUpdate(GameTimestamp timestamp)
   {
        int hours = timestamp.hour;
        int minutes = timestamp.minute;

        string prefix = " AM";

        if(hours > 12)
        {
            prefix = " PM";

            hours -= 12;
        }

        timeText.text = hours + ":" + minutes.ToString("00") + prefix;

        int day = timestamp.day;
        string season = timestamp.season.ToString();
        string dayOfTheWeek = timestamp.GetDayOfTheWeek().ToString();

        dateText.text = season + " " + day + " (" +dayOfTheWeek + ")";
   }
}
