using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour, IPointerEnterHandler,  IPointerExitHandler, IPointerClickHandler
{
    ItemData itemToDisplay;

    int quantity;

    public Image itemDisplayImage;

    public enum InventoryType
    {
        Item, Tool
    }
    public InventoryType inventoryType;
    
    int slotIndex;
    public void Display(ItemSlotData itemSlots)
   {
    itemToDisplay = itemSlots.itemData;

    quantity = itemSlots.quantity;

    if(itemToDisplay != null)
    {
        itemDisplayImage.sprite = itemToDisplay.thumbnail;

        itemDisplayImage.gameObject.SetActive(true);

        return;
    }

    itemDisplayImage.gameObject.SetActive(false);

   }

    public virtual void OnPointerClick(PointerEventData eventData)
    {
       InventoryManager.Instance.InventoryToHand(slotIndex, inventoryType);
    } 

    public void AssignIndex(int index)
    {
        this.slotIndex = index;
    }

    public void OnPointerEnter(PointerEventData eventData)
   {
        UIManager.Instance.DisplayItemInfo(itemToDisplay);
   }

    public void OnPointerExit(PointerEventData eventData)
   {
       UIManager.Instance.DisplayItemInfo(null);
   }
}
