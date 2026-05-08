using UnityEngine;
using UnityEngine.UI;

public class CookingStation : MonoBehaviour
{
    [Header("Recipe Settings")]
    public string requiredIngredient; // e.g., "Lettuce"
    public int requiredAmount = 5;
    public float cookTime = 3f;

    [Header("UI")]
    public Text ingredientDisplay; // Shows "0/5 🥬"
    public GameObject cookingIcon; // Timer icon
    public GameObject readyIcon;   // Pan icon

    private float timer = 0f;
    private int currentServed = 0;
    private bool isCooking = false;
    private bool isReadyToServe = false;

    void Update()
    {
        UpdateUI();
        if (isCooking)
        {
            timer += Time.deltaTime;
            if (timer >= cookTime)
            {
                FinishCooking();
            }
        }
    }

    public void StartCooking()
    {
        if (GameManager.Instance.HasIngredient(requiredIngredient, requiredAmount))
        {
            GameManager.Instance.RemoveIngredient(requiredIngredient, requiredAmount);
            isCooking = true;
            isReadyToServe = false;
            currentServed = 0;
            timer = 0f;
            cookingIcon.SetActive(true);
            readyIcon.SetActive(false);
        }
        else
        {
            Debug.Log("Not enough ingredients!");
        }
    }

    void FinishCooking()
    {
        isCooking = false;
        isReadyToServe = true;
        cookingIcon.SetActive(false);
        readyIcon.SetActive(true);
        Debug.Log("Cooking Complete!");
    }

    public void ServeCustomer()
    {
        if (!isReadyToServe) return;

        if (currentServed < GameManager.Instance.cookingCapacity)
        {
            currentServed++;
            if (currentServed >= GameManager.Instance.cookingCapacity)
            {
                isReadyToServe = false;
                readyIcon.SetActive(false);
                Debug.Log("Batch Finished. Cook again.");
            }
        }
    }

    void UpdateUI()
    {
        if (isCooking)
        {
            ingredientDisplay.text = $"Cooking... {timer:F1}s";
        }
        else if (isReadyToServe)
        {
            ingredientDisplay.text = $"Ready to Serve ({currentServed}/{GameManager.Instance.cookingCapacity})";
        }
        else
        {
            ingredientDisplay.text = $"Need {requiredAmount} {requiredIngredient}";
        }
    }
}