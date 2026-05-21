using UnityEngine;

public class CustomerOrder : MonoBehaviour
{
    [Header("Wanted Food")]
    public ItemData wantedItem;

    public void TryServe()
    {
        ItemData equipped =
            InventoryManager.Instance.GetEquippedSlotItem(
                InventorySlot.InventoryType.Item
            );

        // NOTHING IN HAND
        if(equipped == null)
        {
            Debug.Log("Player holding nothing");
            return;
        }

        Debug.Log("PLAYER HOLDING: " + equipped.itemName);

        if(wantedItem != null)
        {
            Debug.Log("CUSTOMER WANTS: " + wantedItem.itemName);
        }

        // CORRECT FOOD
        if(equipped == wantedItem)
        {
            Debug.Log("Customer served!");

            // REMOVE FOOD FROM PLAYER
            InventoryManager.Instance.ConsumeItem(
                InventoryManager.Instance.GetEquippedSlot(
                    InventorySlot.InventoryType.Item
                )
            );

            InventoryManager.Instance.RenderHand();

            // CUSTOMER LEAVES
            CustomerAI ai =
                GetComponent<CustomerAI>();

            if(ai != null)
            {
                ai.LeaveHappy();
            }
        }
        else
        {
            Debug.Log("Wrong order!");
        }
    }
}