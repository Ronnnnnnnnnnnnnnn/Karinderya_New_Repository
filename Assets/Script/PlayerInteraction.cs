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

    MissionBoard selectedMissionBoard = null;

    public GameObject interactUI;

    void Start()
    {
        movement =
            transform.parent.GetComponent<Movement>();
    }

    void Update()
    {

        if (PauseMenu.InMainMenu)
            return;

        if (MissionNotebookUI.IsOpen)
        {
            ClearSelections();
            return;
        }

        Ray ray =
            Camera.main.ScreenPointToRay(
                new Vector3(
                    Screen.width / 2f,
                    Screen.height / 2f
                )
            );

        RaycastHit hit;

        if (Physics.Raycast(
            ray,
            out hit,
            6f))
        {
            OnInteractableHit(hit);
        }
        else
        {
            ClearSelections();
        }
    }

    void OnInteractableHit(RaycastHit hit)
    {
        Collider other = hit.collider;

        if (other.CompareTag("Land"))
        {
            Land land =
                other.GetComponent<Land>();

            if (land != null)
            {

                selectedInteractable = null;
                selectedPot = null;
                selectedBuffet = null;
                selectedCustomer = null;
                selectedMissionBoard = null;

                HideUI();
                SelectLand(land);
            }

            return;
        }

        DeselectLand();

        InteractableObject interactable =
            other.GetComponentInParent<InteractableObject>();

        bool isHeldModel =
            InventoryManager.Instance != null &&
            InventoryManager.Instance.handPoint != null &&
            other.transform.IsChildOf(
                InventoryManager.Instance.handPoint);

        if (!isHeldModel &&
            (interactable != null ||
             other.CompareTag("Item")))
        {
            selectedInteractable = interactable;

            selectedPot = null;
            selectedBuffet = null;
            selectedCustomer = null;
            selectedMissionBoard = null;

            ShowUI();

            return;
        }

        Pot pot =
            other.GetComponentInParent<Pot>();

        if (pot != null)
        {
            selectedPot = pot;

            selectedInteractable = null;
            selectedBuffet = null;
            selectedCustomer = null;
            selectedMissionBoard = null;

            ShowUI();

            Debug.Log(
                "[INTERACT] Looking at Pot"
            );

            return;
        }

        BuffetContainer buffet =
            other.GetComponentInParent<BuffetContainer>();

        if (buffet != null)
        {
            selectedBuffet = buffet;

            selectedInteractable = null;
            selectedPot = null;
            selectedCustomer = null;
            selectedMissionBoard = null;

            HideUI();

            Debug.Log(
                "[INTERACT] Looking at Buffet: " +
                buffet.name
            );

            return;
        }

        MissionBoard missionBoard =
            other.GetComponentInParent<MissionBoard>();

        if (missionBoard != null)
        {
            selectedMissionBoard = missionBoard;

            selectedInteractable = null;
            selectedPot = null;
            selectedBuffet = null;
            selectedCustomer = null;

            ShowUI();

            Debug.Log(
                "[INTERACT] Looking at Mission Board"
            );

            return;
        }

        CustomerOrder customer =
            other.GetComponentInParent<CustomerOrder>();

        if (customer != null)
        {
            selectedCustomer = customer;

            selectedInteractable = null;
            selectedPot = null;
            selectedBuffet = null;
            selectedMissionBoard = null;

            HideUI();

            Debug.Log(
                "[INTERACT] Looking at Customer"
            );

            return;
        }

        ClearSelections();
    }

    void ClearSelections()
    {
        selectedInteractable = null;

        selectedPot = null;

        selectedBuffet = null;

        selectedCustomer = null;

        selectedMissionBoard = null;

        DeselectLand();

        HideUI();
    }

    void DeselectLand()
    {
        if (selectedLand != null)
        {
            selectedLand.Select(false);

            selectedLand.ShowTimer(false);

            selectedLand = null;
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

        land.ShowTimer(true);
    }

    public void Interact()
    {
        Debug.Log(
            "[PLAYER] Left Click"
        );

        if (selectedLand != null)
        {
            selectedLand.Interact();

            return;
        }

        if (selectedPot != null)
        {
            selectedPot.Interact();

            return;
        }

        if (selectedBuffet != null)
        {
            selectedBuffet.Interact();

            return;
        }

        if (selectedMissionBoard != null)
        {
            selectedMissionBoard.Open();

            return;
        }

        if (selectedCustomer != null)
        {
            selectedCustomer.TryServe();

            return;
        }

        if (selectedInteractable != null)
        {
            selectedInteractable.Pickup();

            HideUI();

            return;
        }

        Debug.Log(
            "[PLAYER] Nothing to interact"
        );
    }

    public void ItemInteract()
    {
        Debug.Log(
            "[PLAYER] E Interact"
        );

        if (CookingUIManager.Instance != null &&
            CookingUIManager.Instance.IsOpen)
        {
            CookingUIManager.Instance.CloseAll();

            HideUI();

            return;
        }

        if (selectedMissionBoard != null)
        {
            selectedMissionBoard.Open();

            return;
        }

        if (selectedBuffet != null)
        {
            selectedBuffet.Interact();

            return;
        }

        if (selectedPot != null)
        {
            HideUI();

            selectedPot.Interact();

            return;
        }

        if (InventoryManager.Instance.SlotEquipped(
            InventorySlot.InventoryType.Item))
        {
            InventoryManager.Instance.HandToInventory(
                InventorySlot.InventoryType.Item
            );

            HideUI();

            return;
        }

        if (selectedInteractable != null)
        {
            selectedInteractable.Pickup();

            HideUI();

            return;
        }
    }

    void ShowUI()
    {
        if (interactUI != null)
        {
            if (CookingUIManager.Instance != null &&
                CookingUIManager.Instance.IsOpen)
            {
                interactUI.SetActive(false);

                return;
            }

            interactUI.SetActive(true);
        }
    }

    void HideUI()
    {
        if (interactUI != null)
        {
            interactUI.SetActive(false);
        }
    }
}
