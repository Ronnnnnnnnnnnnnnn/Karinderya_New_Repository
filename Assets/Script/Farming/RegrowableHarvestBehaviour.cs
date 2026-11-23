using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegrowableHarvestBehaviour : InteractableObject
{
    CropBehaviour parentCrop;

    public void SetParent(CropBehaviour parentCrop)
    {
        this.parentCrop = parentCrop;
    }

    public override void Pickup()
    {
        if (parentCrop == null ||
            item == null ||
            InventoryManager.Instance == null)
        {
            return;
        }

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

        parentCrop.Regrow();

    }
}
