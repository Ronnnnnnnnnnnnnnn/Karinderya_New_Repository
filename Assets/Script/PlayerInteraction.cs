using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public Land selectedLand = null;

    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 6f))
        {
            Land land = hit.collider.GetComponent<Land>();
            if (land != null)
            {
                if (selectedLand != null && selectedLand != land)
                    selectedLand.Select(false);

                selectedLand = land;
                selectedLand.Select(true);
            }
            else
            {
                ClearSelection();
            }
        }
        else
        {
            ClearSelection();
        }

        if (Input.GetButtonDown("Fire1") && selectedLand != null)
        {
            selectedLand.Interact();
        }
    }

    void ClearSelection()
    {
        if (selectedLand != null)
        {
            selectedLand.Select(false);
            selectedLand = null;
        }
    }
}