using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    public ItemData item;

    // Set by CropBehaviour so a picked-up crop is also removed from the land
    public System.Action onPickedUp;

    public virtual void Pickup()
    {
        InventoryManager.Instance.EquipHandSlot(item);

        InventoryManager.Instance.RenderHand();

        onPickedUp?.Invoke();

        Destroy(gameObject);
    }
}