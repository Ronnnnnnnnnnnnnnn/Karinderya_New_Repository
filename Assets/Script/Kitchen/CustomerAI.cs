using UnityEngine;

public class CustomerAI : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2f;

    [Header("Targets")]
    public Transform counterPoint;

    public Transform exitPoint;

    [Header("Order")]
    public ItemData wantedFood;

    [Header("Patience")]
    public float patienceTime = 120f;

    private float timer;

    private bool waiting;

    private bool leaving;

    void Update()
    {
        if(counterPoint == null)
            return;

        if(!waiting && !leaving)
        {
            MoveToCounter();
        }
        else if(waiting)
        {
            Wait();
        }
        else if(leaving)
        {
            Leave();
        }
    }

    void MoveToCounter()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            counterPoint.position,
            moveSpeed * Time.deltaTime
        );

        if(Vector3.Distance(
            transform.position,
            counterPoint.position
        ) < 0.2f)
        {
            waiting = true;

            Debug.Log(name + " waiting for order");
        }
    }

    void Wait()
    {
        timer += Time.deltaTime;

        if(timer >= patienceTime)
        {
            Debug.Log(name + " angry and leaving");

            leaving = true;

            waiting = false;
        }
    }

    public void ServeCustomer(BuffetContainer buffet)
    {
        bool served =
            buffet.TakeServing(wantedFood);

        if(served)
        {
            Debug.Log(name + " served!");

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

        if(Vector3.Distance(
            transform.position,
            exitPoint.position
        ) < 0.2f)
        {
            Destroy(gameObject);
        }
    }
}