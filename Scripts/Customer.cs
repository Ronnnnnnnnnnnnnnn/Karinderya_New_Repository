using UnityEngine;
using UnityEngine.UI;

public class Customer : MonoBehaviour
{
    [Header("Settings")]
    public float patienceTime = 10f;
    public float reward = 50f;

    [Header("UI")]
    public GameObject orderIcon;
    public GameObject patienceTimer; // A simple bar or text

    private float timer = 0f;
    private bool isLeaving = false;

    void Start()
    {
        timer = patienceTime;
        orderIcon.SetActive(true);
        patienceTimer.SetActive(true);
    }

    void Update()
    {
        if (!isLeaving)
        {
            timer -= Time.deltaTime;
            UpdatePatienceUI();

            if (timer <= 0)
            {
                LeaveCustomer();
            }
        }
    }

    public void OrderFood()
    {
        // In a real game, check if CookingStation has food
        // For prototype, we assume player clicks "Serve" on CookingStation
        // This script just handles the customer side
    }

    public void Pay()
    {
        GameManager.Instance.AddMoney(reward);
        LeaveCustomer();
    }

    void LeaveCustomer()
    {
        isLeaving = true;
        Destroy(gameObject);
    }

    void UpdatePatienceUI()
    {
        // Simple text update
        patienceTimer.GetComponent<Text>().text = "Patience: " + timer.ToString("F1");
        // Change color based on time
        if (timer < 3f) patienceTimer.GetComponent<Text>().color = Color.red;
    }
}