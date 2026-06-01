using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RecipeSlotUI : MonoBehaviour
{
    [Header("Dish")]
    public Image dishSprite;

    public TMP_Text dishName;

    [Header("Ingredients")]
    public Image[] ingredientSprites;

    public void Setup(RecipeData recipe)
    {
        // DISH

        dishSprite.sprite =
            recipe.resultDish.itemSprite;

        dishName.text =
            recipe.resultDish.itemName;

        // HIDE ALL INGREDIENT SLOTS

        for(int i = 0;
            i < ingredientSprites.Length;
            i++)
        {
            ingredientSprites[i]
                .gameObject
                .SetActive(false);
        }

        // SHOW RECIPE INGREDIENTS

        for(int i = 0;
            i < recipe.ingredients.Length;
            i++)
        {
            ingredientSprites[i]
                .gameObject
                .SetActive(true);

            ingredientSprites[i].sprite =
                recipe.ingredients[i].itemSprite;
        }
    }
}