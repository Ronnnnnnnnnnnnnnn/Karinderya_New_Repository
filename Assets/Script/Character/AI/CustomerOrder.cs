using UnityEngine;

public class CustomerOrder : MonoBehaviour
{
    public ItemData wantedItem;

    public void TryServe()
    {
        ItemData equipped =
            InventoryManager.Instance.equippedItem;

        // NOTHING IN HAND
        if (equipped == null)
        {
            Debug.Log("Player holding nothing");
            return;
        }

        Debug.Log("PLAYER HOLDING: " + equipped.itemName);

        if(wantedItem != null)
        {
            Debug.Log("CUSTOMER WANTS: " + wantedItem.itemName);
        }
        else
        {
            Debug.Log("CUSTOMER HAS NO ORDER");
        }

        // CORRECT FOOD
        if (equipped == wantedItem)
        {
            Debug.Log("Customer served!");

            InventoryManager.Instance.equippedItem = null;

            InventoryManager.Instance.RenderHand();

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