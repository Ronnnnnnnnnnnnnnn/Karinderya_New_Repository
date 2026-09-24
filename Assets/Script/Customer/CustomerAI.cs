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
        anim = GetComponentInChildren<Animator>();
        order = GetComponent<CustomerOrder>();

        targetPoint = counterPoint;

        Debug.Log("[AI] Walking to counter");
    }

    void Update()
    {
        if (targetPoint == null)
        {
            return;
        }

        MoveToTarget();
        UpdateAnimation();
    }

    void MoveToTarget()
    {
        Vector3 direction =
            targetPoint.position -
            transform.position;

        direction.y = 0f;

        float distance = direction.magnitude;

        if (distance <= stoppingDistance)
        {
            isMoving = false;

            UpdateAnimation();

            if (!leaving && lookPoint != null)
            {
                Vector3 lookDir =
                    lookPoint.position -
                    transform.position;

                lookDir.y = 0f;

                if (lookDir != Vector3.zero)
                {
                    transform.rotation =
                        Quaternion.LookRotation(lookDir);
                }
            }

            if (!reachedCounter && !leaving)
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

        isMoving = true;

        direction.Normalize();

        transform.forward = direction;

        transform.position +=
            direction *
            moveSpeed *
            Time.deltaTime;
    }

    public void LeaveHappy()
    {
        if (NotificationManager.Instance != null)
        {
            NotificationManager.Instance.ShowMessage(
                "Customer Happy!"
            );
        }

        // 🔊 CUSTOMER HAPPY SOUND
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(
                AudioManager.Instance.customerHappySound
            );
        }

        Debug.Log(
            "[AI] Leaving happy"
        );

        leaving = true;

        targetPoint = exitPoint;
    }

    public void LeaveAngry()
    {
        if (NotificationManager.Instance != null)
        {
            NotificationManager.Instance.ShowMessage(
                "Customer Angry!"
            );
        }

        // 🔊 CUSTOMER ANGRY SOUND
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(
                AudioManager.Instance.customerAngrySound
            );
        }

        Debug.Log(
            "[AI] Leaving angry"
        );

        leaving = true;

        targetPoint = exitPoint;
    }

    void UpdateAnimation()
    {
        if (anim == null)
        {
            return;
        }

        anim.SetFloat(
            "Speed",
            isMoving ? 1f : 0f
        );
    }
}