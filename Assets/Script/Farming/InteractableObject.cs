using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    public ItemData item;

    public System.Action onPickedUp;

    public virtual void Pickup()
    {
        if (item == null || InventoryManager.Instance == null)
            return;

        if (InventoryManager.Instance.SlotEquipped(
                InventorySlot.InventoryType.Item))
        {
            if (!InventoryManager.Instance.HandToInventory(
                    InventorySlot.InventoryType.Item))
            {
                if (NotificationManager.Instance != null)
                {
                    NotificationManager.Instance.ShowMessage(
                        "Inventory Full!"
                    );
                }

                return;
            }
        }

        InventoryManager.Instance.EquipHandSlot(item);
        InventoryManager.Instance.RenderHand();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(
                AudioManager.Instance.pickupSound
            );
        }

        onPickedUp?.Invoke();

        Destroy(gameObject);
    }
}
