using System.Collections;
using UnityEngine;
using TMPro;

public class Pot : MonoBehaviour
{
    [Header("Recipes")]
    public RecipeData[] availableRecipes;

    [Header("Fallback catalog (all dishes)")]
    public RecipeBookCatalog recipeCatalog;

    [Header("Legacy single recipe")]
    public RecipeData recipe;

    [Header("Cooking")]
    public float cookTime = 10f;

    bool cooking;
    bool cooked;

    [Header("Dish Visual")]
    public SpriteRenderer dishRenderer;

    [Header("Timer Visual")]
    public SpriteRenderer timerRenderer;

    [Header("Timer Sprites")]
    public Sprite[] numberSprites;

    public Sprite cookedSprite;

    [Header("Ingredient Counter")]
    public TextMeshPro ingredientText;

    RecipeData activeRecipe;
    ItemData cookedDish;

    void Start()
    {
        HideDish();
        HideTimer();
        UpdateIngredientText();

        if (recipeCatalog == null)
        {
            RecipeBookCatalog[] catalogs = Resources.FindObjectsOfTypeAll<RecipeBookCatalog>();
            if (catalogs.Length > 0)
                recipeCatalog = catalogs[0];
        }
    }

    public RecipeData[] GetRecipes()
    {
        if (availableRecipes != null && availableRecipes.Length > 0)
            return availableRecipes;

        if (recipeCatalog != null && recipeCatalog.recipes != null && recipeCatalog.recipes.Length > 0)
            return recipeCatalog.recipes;

        if (recipe != null)
            return new RecipeData[] { recipe };

        return new RecipeData[0];
    }

    public bool CanStartCooking()
    {
        return !cooking && !cooked;
    }

    public void Interact()
    {
        if (cooked)
        {
            GiveDish();
            return;
        }

        if (cooking)
            return;
    }

    public bool TryStartCooking(RecipeData recipeToCook)
    {
        if (!CanStartCooking() || recipeToCook == null)
            return false;

        if (!InventoryManager.Instance.HasItems(recipeToCook.ingredients))
            return false;

        InventoryManager.Instance.ConsumeItems(recipeToCook.ingredients);
        activeRecipe = recipeToCook;

        if (ingredientText != null)
            ingredientText.gameObject.SetActive(false);

        StartCoroutine(CookRoutine());
        return true;
    }

    IEnumerator CookRoutine()
    {
        cooking = true;

        float duration = activeRecipe != null ? activeRecipe.cookTime : cookTime;
        float timer = duration;

        ShowTimer();

        while (timer > 0)
        {
            int rounded = Mathf.CeilToInt(timer);
            UpdateTimerSprite(rounded);
            yield return new WaitForSeconds(1f);
            timer--;
        }

        FinishCooking();
    }

    void FinishCooking()
    {
        cooking = false;
        cooked = true;
        cookedDish = activeRecipe != null ? activeRecipe.resultDish : null;

        HideTimer();
        ShowDish();

        string dishName = cookedDish != null ? cookedDish.itemName : "Dish";
        NotificationManager.Instance.ShowMessage(dishName + " Cooked!");
    }

    void GiveDish()
    {
        if (cookedDish == null)
            return;

        InventoryManager.Instance.AddItem(cookedDish);
        NotificationManager.Instance.ShowMessage(cookedDish.itemName + " Taken");

        cooked = false;
        activeRecipe = null;
        cookedDish = null;

        HideDish();
        UpdateIngredientText();

        if (ingredientText != null)
            ingredientText.gameObject.SetActive(true);
    }

    void ShowDish()
    {
        if (dishRenderer == null || cookedDish == null)
            return;

        dishRenderer.sprite = cookedDish.itemSprite;
        dishRenderer.gameObject.SetActive(true);
    }

    void HideDish()
    {
        if (dishRenderer == null)
            return;

        dishRenderer.gameObject.SetActive(false);
    }

    void ShowTimer()
    {
        if (timerRenderer != null)
            timerRenderer.gameObject.SetActive(true);
    }

    void HideTimer()
    {
        if (timerRenderer != null)
            timerRenderer.gameObject.SetActive(false);
    }

    void UpdateTimerSprite(int number)
    {
        if (timerRenderer == null || numberSprites == null)
            return;

        if (number < 0 || number >= numberSprites.Length)
            return;

        timerRenderer.sprite = numberSprites[number];
    }

    void UpdateIngredientText()
    {
        if (ingredientText == null)
            return;

        RecipeData[] recipes = GetRecipes();

        if (recipes.Length == 0)
        {
            ingredientText.text = "NO RECIPE";
            return;
        }

        if (cooking)
        {
            ingredientText.text = "COOKING";
            return;
        }

        if (cooked)
        {
            ingredientText.text = "DONE";
            return;
        }

        ingredientText.text = "Press E";
    }
}
