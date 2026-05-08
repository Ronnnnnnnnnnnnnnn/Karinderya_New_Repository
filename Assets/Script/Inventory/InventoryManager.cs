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

    public ItemData[] tools = new ItemData[8];

    public ItemData equippedTool = null;

    [Header("Items")]
    
    public ItemData[] items = new ItemData[8];

    public ItemData equippedItem  = null;

    public Transform handPoint;

    public void InventoryToHand(int slotIndex, InventorySlot.InventoryType InventoryType)
    {
        if(InventoryType == InventorySlot.InventoryType.Item)
        {
            ItemData itemToEquip = items[slotIndex];

            items[slotIndex] = equippedItem;

            equippedItem = itemToEquip;

            RenderHand();

        }else
        {
            ItemData toolToEquip = tools[slotIndex];

            tools[slotIndex] = equippedTool;

            equippedTool = toolToEquip;
        }

        UIManager.Instance.RenderInventory();
    }

    public void HandToInventory(InventorySlot.InventoryType InventoryType)
    {
        if(InventoryType == InventorySlot.InventoryType.Item)
        {
            for(int i = 0; i < items.Length; i++)
            {
                if(items[i] == null)
                {
                    items[i] = equippedItem;

                    equippedItem = null;

                    break;
                }
            }

            RenderHand();

        }else
        {
             for(int i = 0; i < tools.Length; i++)
            {
                if(tools[i] == null)
                {
                    tools[i] = equippedTool;

                    equippedTool = null;

                    break;
                }
            }
        }
        UIManager.Instance.RenderInventory();
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

        if (equippedItem == null)
        {
            return;
        }

        if (equippedItem.gameModel == null)
        {
            Debug.LogError("ItemData gameModel is missing!");
            return;
        }

        GameObject obj = Instantiate(equippedItem.gameModel, handPoint);

        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;
        obj.transform.localScale = Vector3.one;
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}