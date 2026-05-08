using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Economy")]
    public float money = 100f;
    public float dailyIncome = 0f;

    [Header("Inventory")]
    public List<string> ingredients = new List<string>(); // Stores harvested crops
    public int cookingCapacity = 10; // Max servings per batch

    [Header("Progression")]
    public int playerLevel = 1;
    public int staffCount = 0;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AddMoney(float amount)
    {
        money += amount;
        UIManager.Instance.UpdateMoneyUI(money);
    }

    public void DeductMoney(float amount)
    {
        if (money >= amount)
        {
            money -= amount;
            UIManager.Instance.UpdateMoneyUI(money);
        }
    }

    public void AddIngredient(string ingredientName)
    {
        ingredients.Add(ingredientName);
        UIManager.Instance.UpdateInventoryUI();
    }

    public bool HasIngredient(string ingredientName, int count)
    {
        int currentCount = ingredients.Count(i => i == ingredientName);
        return currentCount >= count;
    }

    public void RemoveIngredient(string ingredientName, int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (ingredients.Contains(ingredientName))
            {
                ingredients.Remove(ingredientName);
            }
        }
        UIManager.Instance.UpdateInventoryUI();
    }
}