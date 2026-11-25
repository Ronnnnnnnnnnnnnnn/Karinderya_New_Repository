using UnityEngine;
using UnityEngine.AI;

public class CustomerAI : MonoBehaviour
{
    [Header("Points")]
    public Transform counterPoint;
    public Transform exitPoint;
    public Transform lookPoint;

    [Header("Movement")]
    public float moveSpeed = 2f;
    public float stoppingDistance = 0.2f;

    [Header("Pathing")]
    [Tooltip("Follow the baked NavMesh so customers go around the house walls " +
             "instead of straight through them. Falls back to a straight line " +
             "when no NavMesh is baked.")]
    public bool useNavMesh = true;

    [Tooltip("How far from the customer or target to look for NavMesh.")]
    public float navSampleDistance = 3f;

    [Header("Visual")]
    public Transform visualModel;

    [HideInInspector]
    public CustomerSpawner spawner;

    [HideInInspector]
    public int serviceSpotIndex = -1;

    Animator anim;
    Transform targetPoint;
    bool leaving;
    bool reachedCounter;
    bool isMoving;
    bool releasedServiceSpot;
    CustomerOrder order;
    Collider interactionCollider;
    Vector3[] pathCorners;
    int pathIndex;

    void Start()
    {
        anim = GetComponentInChildren<Animator>();
        order = GetComponent<CustomerOrder>();
        interactionCollider = GetComponent<Collider>();

        SetTarget(counterPoint);

        Debug.Log("[AI] Walking to counter");
    }

    void SetTarget(Transform target)
    {
        targetPoint = target;
        BuildPath();
    }

    void BuildPath()
    {
        pathCorners = null;
        pathIndex = 0;

        if (!useNavMesh || targetPoint == null)
            return;

        if (!NavMesh.SamplePosition(
                transform.position,
                out NavMeshHit startHit,
                navSampleDistance,
                NavMesh.AllAreas))
        {
            return;
        }

        if (!NavMesh.SamplePosition(
                targetPoint.position,
                out NavMeshHit endHit,
                navSampleDistance,
                NavMesh.AllAreas))
        {
            return;
        }

        NavMeshPath path = new NavMeshPath();

        if (!NavMesh.CalculatePath(
                startHit.position,
                endHit.position,
                NavMesh.AllAreas,
                path))
        {
            return;
        }

        // A partial path would stop short of the goal; walk straight instead.
        if (path.status != NavMeshPathStatus.PathComplete ||
            path.corners.Length < 2)
        {
            return;
        }

        pathCorners = path.corners;
        pathIndex = 1;
    }

    Vector3 CurrentDestination()
    {
        while (pathCorners != null && pathIndex < pathCorners.Length - 1)
        {
            Vector3 flat = pathCorners[pathIndex] - transform.position;
            flat.y = 0f;

            if (flat.magnitude > stoppingDistance)
                return pathCorners[pathIndex];

            pathIndex++;
        }

        return targetPoint.position;
    }

    void OnDestroy()
    {
        ReleaseServiceSpot();
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
            CurrentDestination() -
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

                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySFX(
                        AudioManager.Instance.customerArrivalSound
                    );
                }

                if (NotificationManager.Instance != null)
                {
                    NotificationManager.Instance.ShowMessage(
                        order != null && order.isRegular
                            ? "Regular At Counter!"
                            : "Customer At Counter!"
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

        if (interactionCollider != null)
            interactionCollider.enabled = false;

        if (order != null)
            order.HideOrderVisuals();

        ReleaseServiceSpot();

        SetTarget(exitPoint);
    }

    public void LeaveAngry()
    {

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

        if (interactionCollider != null)
            interactionCollider.enabled = false;

        if (order != null)
            order.HideOrderVisuals();

        ReleaseServiceSpot();

        SetTarget(exitPoint);
    }

    void ReleaseServiceSpot()
    {
        if (releasedServiceSpot)
            return;

        releasedServiceSpot = true;

        if (spawner != null)
        {
            spawner.CustomerLeft(this);
        }
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
