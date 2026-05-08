using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Movement : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Transform playerObj;
    CharacterController controller;
    public Transform handPoint;

    [Header("Movement")]
    public float moveSpeed = 5f;

    public float jumpStrength = 1.5f;

    [Header("Physics")]
    public float gravity = -9.8f; 
    public float groundedForce = -2f;
    private Vector3 _velocity;

    [Header("Raycast")]
    [SerializeField] float raycastDistance = 6f;
    [SerializeField] LayerMask pushableLayer;
    Ray ray;

    #region Built in Methods

    PlayerInteraction playerInteraction;

    InteractableObject selectedInteractable = null;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        playerInteraction = GetComponentInChildren<PlayerInteraction>();
    }

    void Update()
    {
        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
        ray = Camera.main.ScreenPointToRay(screenCenter);

        if(Input.GetKeyDown(KeyCode.Space))
        {
            ApplyJump();
        }
        Debug.DrawRay(ray.origin, ray.direction * raycastDistance, Color.red);
        
    Interact();

        if(Input.GetKey(KeyCode.RightBracket))
        {
            TimeManager.Instance.Tick();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Item"))
        {
            selectedInteractable = other.GetComponent<InteractableObject>();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Item"))
        {
            selectedInteractable = null;
        }
    }
        

    public void Interact()
    {
        /*if(InventoryManager.Instance.equippedItem != null)
        {
            return;
        }*/

        if (Input.GetButtonDown("Fire1"))
        {
            Debug.Log("Clicked");

            if (Physics.Raycast(ray, out RaycastHit hit, raycastDistance, ~0))
            {
                Land land = hit.collider.GetComponent<Land>();

                if (land != null)
                {
                    land.Interact();
                }
            }
        }
        
        if(Input.GetButtonDown("Fire2"))
            {
                ItemInteract();
            }
    }

    public void ItemInteract()
    {
        if (InventoryManager.Instance.equippedItem != null)
        {
            InventoryManager.Instance.HandToInventory(InventorySlot.InventoryType.Item);
            return;
        }

        if (Physics.Raycast(ray, out RaycastHit hit, raycastDistance, ~0))
        {
            Debug.Log("Hit: " + hit.collider.name);

            InteractableObject item =
                hit.collider.GetComponent<InteractableObject>();

            if (item != null)
            {
                Debug.Log("Picked up: " + item.name);
                item.Pickup();
            }
        }
    }

    void FixedUpdate()
    {
        ApplyMovement();
        ApplyGravity();
    }

    void LateUpdate()
    {
        
    }
    #endregion

    #region Movement Methods

    void ApplyMovement()
    {
        float x;
        float z;
    
            x = Input.GetAxisRaw("Horizontal");
            z = Input.GetAxisRaw("Vertical");
       

        Vector3 moveDir = orientation.forward * z + orientation.right * x;
        moveDir.y = 0;

        controller.Move(moveDir.normalized * moveSpeed * Time.deltaTime);
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
        if(!controller.isGrounded) return;
        _velocity.y = Mathf.Sqrt(jumpStrength * -2f * gravity);
    }

    #endregion

}


