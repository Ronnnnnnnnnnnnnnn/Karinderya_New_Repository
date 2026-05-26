using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cooking/Recipe")]
public class RecipeData : ScriptableObject
{
    [Header("Dish")]
    public string dishName;

    public Sprite dishSprite;

    public ItemData cookedDish;

    [Header("Ingredients")]
    public List<ItemData> requiredIngredients;

    [Header("Cooking")]
    public float cookTime = 10f;
}