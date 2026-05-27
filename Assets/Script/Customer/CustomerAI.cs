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

    void Start()
    {
        anim =
            GetComponentInChildren<Animator>();

        order =
            GetComponent<CustomerOrder>();

        targetPoint = counterPoint;

        Debug.Log(
            "[AI] Walking to counter"
        );
    }

    void Update()
    {
        if(targetPoint == null)
            return;

        MoveToTarget();

        UpdateAnimation();
    }

    // =====================================================
    // MOVE
    // =====================================================

    void MoveToTarget()
    {
        Vector3 direction =
            targetPoint.position -
            transform.position;

        direction.y = 0f;

        float distance =
            direction.magnitude;

        // =========================================
        // REACHED TARGET
        // =========================================

        if(distance <= stoppingDistance)
        {
            isMoving = false;

            UpdateAnimation();

            // FACE COUNTER ONLY AFTER ARRIVING
            if(!leaving &&
                lookPoint != null)
            {
                Vector3 lookDir =
                    lookPoint.position -
                    transform.position;

                lookDir.y = 0f;

                transform.rotation =
                    Quaternion.LookRotation(
                        lookDir
                    );
            }

            // ARRIVED COUNTER
            if(!reachedCounter &&
                !leaving)
            {
                reachedCounter = true;

                Debug.Log(
                    "[AI] Reached counter"
                );

                NotificationManager.Instance.ShowMessage(
                    "Customer At Counter!"
                );

                if(order != null)
                {
                    order.StartWaiting();
                }

                return;
            }

            // EXIT COMPLETE
            if(leaving)
            {
                if(spawner != null)
                {
                    spawner.CustomerLeft();
                }

                Destroy(gameObject);
            }

            return;
        }

        // =========================================
        // MOVING
        // =========================================

        isMoving = true;

        direction.Normalize();

        // FACE MOVEMENT DIRECTION
        transform.forward = direction;

        // MOVE
        transform.position +=
            direction *
            moveSpeed *
            Time.deltaTime;
    }

    // =====================================================
    // HAPPY
    // =====================================================

    public void LeaveHappy()
    {
        NotificationManager.Instance.ShowMessage(
            "Customer Happy!"
        );

        Debug.Log(
            "[AI] Leaving happy"
        );

        leaving = true;

        targetPoint = exitPoint;
    }

    // =====================================================
    // ANGRY
    // =====================================================

    public void LeaveAngry()
    {
        NotificationManager.Instance.ShowMessage(
            "Customer Angry!"
        );

        Debug.Log(
            "[AI] Leaving angry"
        );

        leaving = true;

        targetPoint = exitPoint;
    }

    // =====================================================
    // ANIMATION
    // =====================================================

    void UpdateAnimation()
    {
        if(anim == null)
            return;

        anim.SetFloat(
            "Speed",
            isMoving ? 1f : 0f
        );
    }
}