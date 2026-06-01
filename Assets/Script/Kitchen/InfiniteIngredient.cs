
using UnityEngine;



public class InfiniteIngredient : InteractableObject

{

    public override void Pickup()

    {

        InventoryManager.Instance.AddItem(item);



        UIManager.Instance.RenderInventory();



        Debug.Log("Received " + item.itemName);

    }

}

