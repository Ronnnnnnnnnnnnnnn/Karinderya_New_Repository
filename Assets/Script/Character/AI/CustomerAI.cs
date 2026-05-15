using UnityEngine;

public class CustomerAI : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2f;

    [Header("Targets")]
    public Transform counterPoint;
    public Transform exitPoint;

    [Header("Patience")]
    public float angryTime = 300f;

    [Header("States")]
    public bool waiting = false;
    public bool leaving = false;

    private float waitTimer = 0f;

    private Vector3 targetPosition;

    private Renderer rend;

    void Start()
    {
        rend = GetComponentInChildren<Renderer>();

        // SAFETY CHECK
        if(counterPoint == null)
        {
            Debug.LogError("Counter Point Missing!");
            return;
        }

        targetPosition = counterPoint.position;
    }

    void Update()
    {
        // MOVE TO COUNTER
        if (!waiting && !leaving)
        {
            MoveToCounter();
        }

        // WAITING FOR FOOD
        else if(waiting)
        {
            WaitForOrder();
        }

        // LEAVING
        else if(leaving)
        {
            Leave();
        }
    }

    void MoveToCounter()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, targetPosition) < 0.2f)
        {
            waiting = true;

            Debug.Log(gameObject.name + " waiting for order...");
        }
    }

    void WaitForOrder()
    {
        waitTimer += Time.deltaTime;

        // ANGRY
        if (waitTimer >= angryTime)
        {
            Debug.Log(gameObject.name + " got angry!");

            // TURN RED
            if(rend != null)
            {
                rend.material.color = Color.red;
            }

            leaving = true;
            waiting = false;
        }
    }

    public void LeaveHappy()
    {
        Debug.Log(gameObject.name + " served successfully!");

        leaving = true;
        waiting = false;
    }

    void Leave()
    {
        // NO EXIT POINT
        if(exitPoint == null)
        {
            Destroy(gameObject);
            return;
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            exitPoint.position,
            moveSpeed * Time.deltaTime
        );

        // DESTROY WHEN EXITED
        if(Vector3.Distance(transform.position, exitPoint.position) < 0.2f)
        {
            Destroy(gameObject);
        }
    }
}