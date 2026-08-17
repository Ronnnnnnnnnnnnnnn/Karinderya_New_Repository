using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Recipe/New Recipe")]
public class RecipeData : ScriptableObject
{
    [Header("Recipe Name")]
    public string recipeName;

    [Header("Ingredients")]
    public ItemData[] ingredients;

    [Header("Result Dish")]
    public ItemData resultDish;

    [Header("Cooking")]
    public float cookTime = 10f;

    [Header("Unlock")]
    public bool unlockedByDefault = false;

    public int unlockCost = 0;

    [Header("Customer Reward")]
    public int sellingPrice = 60;
}