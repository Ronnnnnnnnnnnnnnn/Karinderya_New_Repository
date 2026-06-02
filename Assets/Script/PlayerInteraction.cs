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

    Pot selectedPot = null;

    BuffetContainer selectedBuffet = null;

    CustomerOrder selectedCustomer = null;

    public GameObject interactUI;

    void Start()
    {
        movement = transform.parent.GetComponent<Movement>();
    }

    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay( new Vector3(Screen.width / 2f,Screen.height / 2f));
        RaycastHit hit;

        if(Physics.Raycast(ray, out hit, 6f))
        {
            OnInteractableHit(hit);
        }
        else
        {
            ClearSelections();
        }
    }

    // =====================================================
    // DETECT OBJECTS
    // =====================================================

    void OnInteractableHit(RaycastHit hit)
    {
        Collider other = hit.collider;

        // =========================
        // LAND
        // =========================

        if(other.CompareTag("Land"))
        {
            Land land = other.GetComponent<Land>();

            SelectLand(land);

            return;
        }

        // =========================
        // ITEM
        // =========================

        if(other.CompareTag("Item"))
        {
            selectedInteractable = other.GetComponent<InteractableObject>();

            ShowUI();

            return;
        }

        // =========================
        // POT
        // =========================

        Pot pot = other.GetComponentInParent<Pot>();

        if (pot != null || other.CompareTag("Pot"))
        {
            selectedPot = pot != null ? pot : other.GetComponent<Pot>();

            if (selectedPot != null)
            {
                ShowUI();
                Debug.Log("[INTERACT] Looking at Pot");
                return;
            }
        }

        // =========================
        // BUFFET
        // =========================

        if(other.CompareTag("Buffet"))
        {
            selectedBuffet =
                other.GetComponent<BuffetContainer>();

            HideUI();

            Debug.Log("[INTERACT] Looking at Buffet");

            return;
        }

        // =========================
        // CUSTOMER
        // =========================

        if(other.CompareTag("Customer"))
        {
            selectedCustomer =
                other.GetComponent<CustomerOrder>();

            HideUI();

            Debug.Log("[INTERACT] Looking at Customer");

            return;
        }

        ClearSelections();
    }

    // =====================================================
    // CLEAR
    // =====================================================

    void ClearSelections()
    {
        selectedInteractable = null;

        selectedPot = null;

        selectedBuffet = null;

        selectedCustomer = null;

        if(selectedLand != null)
        {
            selectedLand.Select(false);

            selectedLand.ShowTimer(false);

            selectedLand = null;
        }

        HideUI();
    }

    // =====================================================
    // LAND
    // =====================================================

    void SelectLand(Land land)
    {
        if(selectedLand != null)
        {
            selectedLand.Select(false);
        }

        selectedLand = land;

        land.Select(true);

        land.ShowTimer(true);
    }

    // =====================================================
    // LEFT CLICK INTERACT
    // =====================================================

    public void Interact()
    {
        Debug.Log("[PLAYER] Left Click");

        // LAND
        if(selectedLand != null)
        {
            selectedLand.Interact();

            return;
        }

        // POT
        if(selectedPot != null)
        {
            selectedPot.Interact();

            return;
        }

        // BUFFET
        if(selectedBuffet != null)
        {
            selectedBuffet.Interact();

            return;
        }

        // CUSTOMER
        if(selectedCustomer != null)
        {
            selectedCustomer.TryServe();

            return;
        }

        Debug.Log("[PLAYER] Nothing to interact");
    }

    // =====================================================
    // E PICKUP ONLY
    // =====================================================

    public void ItemInteract()
    {
        Debug.Log("[PLAYER] Pickup Key");

        if (CookingUIManager.Instance != null && CookingUIManager.Instance.IsOpen)
        {
            CookingUIManager.Instance.CloseAll();
            return;
        }

        if (selectedPot != null)
        {
            CookingUIManager.EnsureInstance().OpenRecipeBook(selectedPot);
            return;
        }

        if(InventoryManager.Instance.SlotEquipped(
            InventorySlot.InventoryType.Item))
        {
            InventoryManager.Instance.HandToInventory(
                InventorySlot.InventoryType.Item
            );

            return;
        }

        if(selectedInteractable != null)
        {
            selectedInteractable.Pickup();

            return;
        }
    }

    // =====================================================
    // UI
    // =====================================================

    void ShowUI()
    {
        if(interactUI != null)
        {
            interactUI.SetActive(true);
        }
    }

    void HideUI()
    {
        if(interactUI != null)
        {
            interactUI.SetActive(false);
        }
    }
}