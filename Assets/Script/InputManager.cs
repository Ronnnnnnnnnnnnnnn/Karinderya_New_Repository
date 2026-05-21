using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private PlayerInput playerInput;

    private PlayerInput.PlayerActions player;

    private PlayerMovement1 playerMovement;

    private PlayerLook look;

    void Awake()
    {
        playerInput = new PlayerInput();

        player = playerInput.Player;

        playerMovement = GetComponent<PlayerMovement1>();

        look = GetComponent<PlayerLook>();

        player.Jump.performed += ctx => playerMovement.Jump();
    }


    void FixedUpdate()
    {
         playerMovement.ProcessMove(player.Move.ReadValue<Vector2>());
    }

    public void LateUpdate()
    {
        look.ProcessLook(player.Look.ReadValue<Vector2>());
    }

    private void Enable()
    {
        player.Enable();
    }

    private void OnDisable()
    {
        player.Disable();
    }
}
