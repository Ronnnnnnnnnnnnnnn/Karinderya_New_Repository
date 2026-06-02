using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [Header("Reference")]
    Movement movement;

    [Header("Mouse Input")]
    public float senX;
    public float senY;
    float xRot, yRot;
    Transform orientation;
    Transform playerObject;
    public Transform cam;
    bool isCursorLocked = true;

    void Awake()
    {
        movement = GetComponent<Movement>();
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        orientation = movement.orientation;
        playerObject = movement.playerObj;
        CameraParent();
    }
    
#region Movement Methods
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P) && !ShouldUseFreeCursor())
            isCursorLocked = !isCursorLocked;

        RefreshCursorState();
    }

    public void RefreshCursorState()
    {
        if (ShouldUseFreeCursor())
        {
            EnableCursor();
            return;
        }

        if (isCursorLocked)
            DisableCursor();
        else
            EnableCursor();
    }

    bool ShouldUseFreeCursor()
    {
        if (PauseMenu.Pause)
            return true;

        if (UIManager.Instance != null && UIManager.Instance.IsMenuOpen)
            return true;

        if (CookingUIManager.Instance != null && CookingUIManager.Instance.IsOpen)
            return true;

        return false;
    }

    void LateUpdate()
    {
        if (ShouldUseFreeCursor())
            return;

        float mouseX = Input.GetAxisRaw("Mouse X");
        float mouseY = Input.GetAxisRaw("Mouse Y");

        yRot += mouseX;
        xRot -= mouseY;

        xRot = Mathf.Clamp(xRot, -90f, 90f);

        cam.rotation = Quaternion.Euler(xRot, yRot, 0);
        orientation.rotation = Quaternion.Euler(0, yRot, 0);
        playerObject.rotation = Quaternion.Euler(0, yRot, 0);
    }

    void CameraParent()
    {
        Camera mainCam = Camera.main;
        mainCam.transform.parent = cam;
        mainCam.transform.localPosition = Vector3.zero;
        mainCam.transform.localRotation = Quaternion.identity;
    }
    public void EnableCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void DisableCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    #endregion
}