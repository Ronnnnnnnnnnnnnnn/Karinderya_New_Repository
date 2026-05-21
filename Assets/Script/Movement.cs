using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Movement : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Transform playerObj;
    public Transform handPoint;

    private CharacterController controller;

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpStrength = 1.5f;

    [Header("Physics")]
    public float gravity = -9.8f;
    public float groundedForce = -2f;

    private Vector3 _velocity;

    [Header("Raycast")]
    [SerializeField] float raycastDistance = 6f;

    private Ray ray;

    PlayerInteraction playerInteraction;

    InteractableObject selectedInteractable = null;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        playerInteraction = GetComponentInChildren<PlayerInteraction>();
    }

    void Update()
    {
        // Create ray from camera forward
        ray = new Ray(
            Camera.main.transform.position,
            Camera.main.transform.forward
        );

        // Draw ray for debugging
        Debug.DrawRay(ray.origin, ray.direction * raycastDistance, Color.red);

        // Jump
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ApplyJump();
        }

        // Interactions
        Interact();

        // Time test
        if (Input.GetKey(KeyCode.RightBracket))
        {
            TimeManager.Instance.Tick();
        }
    }

    void FixedUpdate()
    {
        ApplyMovement();
        ApplyGravity();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Item"))
        {
            selectedInteractable =
                other.GetComponent<InteractableObject>();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Item"))
        {
            selectedInteractable = null;
        }
    }

    // =========================
    // INTERACTION
    // =========================

    public void Interact()
    {
        // LEFT CLICK
        if (Input.GetButtonDown("Fire1"))
        {
            Debug.Log("LEFT CLICK");

            if (Physics.Raycast(ray, out RaycastHit hit, raycastDistance))
            {
                Debug.Log("HIT: " + hit.collider.name);

                Land land = hit.collider.GetComponent<Land>();

                if (land != null)
                {
                    land.Interact();
                }
            }
        }

        // E KEY
        if (Input.GetKeyDown(KeyCode.E))
        {
            ItemInteract();
        }
    }

  public void ItemInteract()
{
    Debug.Log("E PRESSED");

    if (Physics.Raycast(ray, out RaycastHit hit, raycastDistance))
    {
        Debug.Log("RAY HIT: " + hit.collider.name);

        // =========================
        // POT INTERACTION
        // =========================

        Pot pot = hit.collider.GetComponent<Pot>();

        if (pot != null)
        {
            // TAKE COOKED FOOD
            if (pot.HasCookedFood())
            {
                ItemData cookedFood = pot.TakeItem();

                InventoryManager.Instance.equippedItem = cookedFood;

                InventoryManager.Instance.RenderHand();

                Debug.Log("Picked up cooked food!");

                return;
            }

            Debug.Log("Pot has no cooked food yet.");
            return;
        }

        CustomerOrder customer =
    hit.collider.GetComponentInParent<CustomerOrder>();

if(customer != null)
{
    customer.TryServe();
    return;
}
        // =========================
        // NORMAL ITEM PICKUP
        // =========================

        InteractableObject item =
            hit.collider.GetComponentInParent<InteractableObject>();

        if (item != null)
        {
            Debug.Log("PICKED UP: " + item.name);

            item.Pickup();
        }
        else
        {
            Debug.Log("NO INTERACTABLE OBJECT FOUND");
        }
    }
    else
    {
        Debug.Log("RAY HIT NOTHING");
    }
}

    // =========================
    // MOVEMENT
    // =========================

    void ApplyMovement()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 moveDir =
            orientation.forward * z +
            orientation.right * x;

        moveDir.y = 0f;

        controller.Move(
            moveDir.normalized *
            moveSpeed *
            Time.deltaTime
        );
    }

    void ApplyGravity()
    {
        if (controller.isGrounded && _velocity.y < 0)
        {
            _velocity.y = groundedForce;
        }

        _velocity.y += gravity * Time.deltaTime;

        controller.Move(_velocity * Time.deltaTime);
    }

    public void ApplyJump()
    {
        if (!controller.isGrounded)
            return;

        _velocity.y =
            Mathf.Sqrt(jumpStrength * -2f * gravity);
    }
}