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

    #region Built in Methods

    PlayerInteraction playerInteraction;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        playerInteraction = GetComponentInChildren<PlayerInteraction>();
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space) && !PauseMenu.Pause)
        {
            ApplyJump();
        }

        Interact();
        HandleMenuShortcuts();
    }

    public void Interact()
    {
        if (PauseMenu.Pause)
            return;

        // Inventory / shop open: clicks belong to the UI, not the world
        if (UIManager.Instance != null && UIManager.Instance.IsMenuOpen)
            return;

        // Cooking UI open: only E (which closes it) is allowed
        bool cookingOpen =
            CookingUIManager.Instance != null &&
            CookingUIManager.Instance.IsOpen;

        if (!cookingOpen && Input.GetButtonDown("Fire1"))
            playerInteraction.Interact();

        if (Input.GetKeyDown(KeyCode.E))
            playerInteraction.ItemInteract();
    }

    void HandleMenuShortcuts()
    {
        if (PauseMenu.Pause || UIManager.Instance == null)
            return;

        if (CookingUIManager.Instance != null && CookingUIManager.Instance.IsOpen)
            return;

        if (Input.GetKeyDown(KeyCode.I))
            UIManager.Instance.ToggleInventoryPanel();

        if (Input.GetKeyDown(KeyCode.B))
            UIManager.Instance.ToggleShop();
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