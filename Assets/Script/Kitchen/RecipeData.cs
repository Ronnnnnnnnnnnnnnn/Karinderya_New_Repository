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
}