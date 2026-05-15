using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    public ItemData item;

    Rigidbody rb;
    Collider col;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
    }

    public virtual void Pickup()
    {
        // Add item to inventory
        InventoryManager.Instance.EquipEmptySlot(item);

        // Disable physics
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        // Disable collider
        if (col != null)
        {
            col.enabled = false;
        }

        // Parent to hand
        Transform hand =
        FindFirstObjectByType<Movement>().handPoint;

        transform.SetParent(hand);

        // Reset transform
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;

        // Render hand item
        InventoryManager.Instance.RenderHand();
    }
}