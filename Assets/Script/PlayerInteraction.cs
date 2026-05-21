using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerInteraction : MonoBehaviour
{
    Movement movement;

    Land selectedLand = null;

    InteractableObject selectedInteractable = null;

    public GameObject interactUI;

    void Start()
    {
        movement = transform.parent.GetComponent<Movement>();
    }

    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 6f))
        {
            OnInteractableHit(hit);
        }
    }

    void OnInteractableHit(RaycastHit hit)
    {
        Collider other = hit.collider;
        
        if(other.tag == "Land")
        {
            Land land = other.GetComponent<Land>();
            SelectLand(land);
            return; 
        }

        if(other.tag == "Item")
        {
            selectedInteractable = other.GetComponent<InteractableObject>();
             if(interactUI != null)
                interactUI.SetActive(true);

            return; 
        }

        if(selectedInteractable != null)
        {
            selectedInteractable = null; 
            if(interactUI != null)
                interactUI.SetActive(false);
        }

        if(selectedLand != null)
        {
            selectedLand.Select(false);
            selectedLand = null;

            if(interactUI != null)
                interactUI.SetActive(false);
        }
    }

        void SelectLand(Land land)
    {
        if (selectedLand != null)
        {
            selectedLand.Select(false);
        }
        
        selectedLand = land; 
        land.Select(true);
    }

    public void Interact()
    {
        /*if(InventoryManager.Instance.SlotEquipped(InventorySlot.InventoryType.Item))
        {
            return;
        }*/

        if(selectedLand != null)
        {
            selectedLand.Interact();
            return; 
        }

        Debug.Log("Not on any land!");
    }

    public void ItemInteract()
    {
        if(InventoryManager.Instance.SlotEquipped(InventorySlot.InventoryType.Item))
        {
            InventoryManager.Instance.HandToInventory(InventorySlot.InventoryType.Item);
            return;
        }

        if (selectedInteractable != null)
        {
            selectedInteractable.Pickup();
        }

    }
}