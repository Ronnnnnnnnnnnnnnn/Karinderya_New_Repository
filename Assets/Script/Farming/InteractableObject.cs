using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    public ItemData item;

    public void Pickup()
    {
        InventoryManager.Instance.equippedItem = item;
        
        InventoryManager.Instance.RenderHand();

        Destroy(gameObject);
    }
}
