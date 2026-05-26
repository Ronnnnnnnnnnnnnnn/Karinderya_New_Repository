using UnityEngine;

public class CustomerAI : MonoBehaviour
{
    [Header("Points")]
    public Transform counterPoint;

    public Transform exitPoint;

    [Header("Movement")]
    public float moveSpeed = 2f;

    public float stoppingDistance = 0.2f;

    Animator anim;

    Transform targetPoint;

    bool leaving = false;

    bool isMoving = false;

    void Start()
    {
        anim = GetComponent<Animator>();

        targetPoint = counterPoint;

        Debug.Log(
            "[AI] Moving to counter"
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
    // MOVEMENT
    // =====================================================

    void MoveToTarget()
    {
        Vector3 direction =
            targetPoint.position -
            transform.position;

        direction.y = 0f;

        float distance =
            direction.magnitude;

        // STOP
        if(distance <= stoppingDistance)
        {
            isMoving = false;

            // EXIT COMPLETE
            if(leaving)
            {
                Debug.Log(
                    "[AI] Customer exited"
                );

                Destroy(gameObject);
            }

            return;
        }

        isMoving = true;

        // ROTATE
        transform.forward =
            direction.normalized;

        // MOVE
        transform.position +=
            direction.normalized *
            moveSpeed *
            Time.deltaTime;
    }

    // =====================================================
    // HAPPY LEAVE
    // =====================================================

    public void LeaveHappy()
    {
        Debug.Log(
            "[AI] Leaving happy"
        );

        leaving = true;

        targetPoint = exitPoint;
    }

    // =====================================================
    // ANGRY LEAVE
    // =====================================================

    public void LeaveAngry()
    {
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
        {
            Debug.LogWarning(
                "[AI] Animator missing!"
            );

            return;
        }

        float speedValue =
            isMoving ? 1f : 0f;

        anim.SetFloat(
            "Speed",
            speedValue
        );

        Debug.Log(
            "[AI] Animation Speed: " +
            speedValue
        );
    }
}