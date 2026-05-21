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
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f));

        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
        ray = Camera.main.ScreenPointToRay(screenCenter);

        if(Input.GetKeyDown(KeyCode.Space))
        {
            ApplyJump();
        }

        Interact();

        if(Input.GetKey(KeyCode.RightBracket))
        {
            TimeManager.Instance.Tick();
        }
    }

    public void Interact()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            playerInteraction.Interact();
        }
        
        if(Input.GetKeyDown(KeyCode.E))
            {
                playerInteraction.ItemInteract();
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