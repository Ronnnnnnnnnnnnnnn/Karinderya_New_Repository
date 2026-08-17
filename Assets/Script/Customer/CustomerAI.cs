using UnityEngine;

public class CustomerAI : MonoBehaviour
{
    [Header("Points")]
    public Transform counterPoint;

    public Transform exitPoint;

    public Transform lookPoint;

    [Header("Movement")]
    public float moveSpeed = 2f;

    public float stoppingDistance = 0.2f;

    [Header("Visual")]
    public Transform visualModel;

    [HideInInspector]
    public CustomerSpawner spawner;

    Animator anim;

    Transform targetPoint;

    bool leaving;

    bool reachedCounter;

    bool isMoving;

    CustomerOrder order;


    // =========================================================
    // START
    // =========================================================

    void Start()
    {
        anim =
            GetComponentInChildren<Animator>();

        order =
            GetComponent<CustomerOrder>();

        // Start walking to counter
        targetPoint = counterPoint;

        Debug.Log(
            "[AI] Walking to counter"
        );
    }


    // =========================================================
    // UPDATE
    // =========================================================

    void Update()
    {
        if (targetPoint == null)
            return;

        MoveToTarget();

        UpdateAnimation();
    }


    // =========================================================
    // MOVE
    // =========================================================

    void MoveToTarget()
    {
        Vector3 direction =
            targetPoint.position -
            transform.position;

        direction.y = 0f;

        float distance =
            direction.magnitude;


        // =====================================================
        // REACHED TARGET
        // =====================================================

        if (distance <= stoppingDistance)
        {
            isMoving = false;

            UpdateAnimation();


            // =================================================
            // FACE COUNTER
            // =================================================

            if (!leaving &&
                lookPoint != null)
            {
                Vector3 lookDir =
                    lookPoint.position -
                    transform.position;

                lookDir.y = 0f;

                if (lookDir != Vector3.zero)
                {
                    transform.rotation =
                        Quaternion.LookRotation(
                            lookDir
                        );
                }
            }


            // =================================================
            // ARRIVED AT COUNTER
            // =================================================

            if (!reachedCounter &&
                !leaving)
            {
                reachedCounter = true;

                Debug.Log(
                    "[AI] Reached counter"
                );

                if (NotificationManager.Instance != null)
                {
                    NotificationManager.Instance.ShowMessage(
                        "Customer At Counter!"
                    );
                }

                if (order != null)
                {
                    order.StartWaiting();
                }

                return;
            }


            // =================================================
            // EXIT COMPLETE
            // =================================================

            if (leaving)
            {
                if (spawner != null)
                {
                    spawner.CustomerLeft();
                }

                Debug.Log(
                    "[AI] Customer destroyed"
                );

                Destroy(gameObject);
            }

            return;
        }


        // =====================================================
        // MOVING
        // =====================================================

        isMoving = true;

        direction.Normalize();

        // Face movement direction
        transform.forward =
            direction;

        // Move
        transform.position +=
            direction *
            moveSpeed *
            Time.deltaTime;
    }


    // =========================================================
    // HAPPY
    // =========================================================

    public void LeaveHappy()
    {
        if (NotificationManager.Instance != null)
        {
            NotificationManager.Instance.ShowMessage(
                "Customer Happy!"
            );
        }

        Debug.Log(
            "[AI] Leaving happy"
        );

        leaving = true;

        targetPoint = exitPoint;
    }


    // =========================================================
    // ANGRY
    // =========================================================

    public void LeaveAngry()
    {
        if (NotificationManager.Instance != null)
        {
            NotificationManager.Instance.ShowMessage(
                "Customer Angry!"
            );
        }

        Debug.Log(
            "[AI] Leaving angry"
        );

        leaving = true;

        targetPoint = exitPoint;
    }


    // =========================================================
    // ANIMATION
    // =========================================================

    void UpdateAnimation()
    {
        if (anim == null)
            return;

        anim.SetFloat(
            "Speed",
            isMoving ? 1f : 0f
        );
    }
}