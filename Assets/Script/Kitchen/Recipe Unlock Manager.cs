using System.Collections.Generic;
using UnityEngine;

public class RecipeUnlockManager : MonoBehaviour
{
    public static RecipeUnlockManager Instance;

    [Header("Recipes Unlocked From Start")]
    public RecipeData[] startingUnlockedRecipes;

    private HashSet<RecipeData> unlockedRecipes =
        new HashSet<RecipeData>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (startingUnlockedRecipes != null)
        {
            foreach (RecipeData recipe in startingUnlockedRecipes)
            {
                if (recipe != null)
                {
                    unlockedRecipes.Add(recipe);

                    Debug.Log(
                        "[RECIPE] Starting unlocked: " +
                        recipe.recipeName
                    );
                }
            }
        }

        if (SaveManager.HasSave)
            LoadFromSave(SaveManager.Data);
    }

    public void WriteSave(SaveData data)
    {
        if (data == null)
            return;

        data.unlockedRecipes = new List<string>();

        foreach (RecipeData recipe in unlockedRecipes)
        {
            if (recipe != null &&
                !string.IsNullOrEmpty(recipe.recipeName))
            {
                data.unlockedRecipes.Add(recipe.recipeName);
            }
        }
    }

    public void LoadFromSave(SaveData data)
    {
        if (data == null ||
            data.unlockedRecipes == null)
        {
            return;
        }

        RecipeBookCatalog catalog = RecipeBookCatalog.Find();

        if (catalog == null || catalog.recipes == null)
            return;

        foreach (string recipeName in data.unlockedRecipes)
        {
            RecipeData recipe = FindRecipe(catalog, recipeName);

            if (recipe != null)
                unlockedRecipes.Add(recipe);
        }
    }

    public bool GrantRecipe(RecipeData recipe)
    {
        if (recipe == null || IsUnlocked(recipe))
            return false;

        unlockedRecipes.Add(recipe);

        BuffetContainer[] buffets =
            FindObjectsByType<BuffetContainer>(
                FindObjectsSortMode.None
            );

        foreach (BuffetContainer buffet in buffets)
        {
            if (buffet != null &&
                buffet.buffetRecipe == recipe)
            {
                buffet.RefreshLockVisual();
            }
        }

        if (NotificationManager.Instance != null)
        {
            NotificationManager.Instance.ShowMessage(
                recipe.recipeName + " Unlocked!"
            );
        }

        return true;
    }

    public RecipeData FindRecipeByName(string recipeName)
    {
        RecipeBookCatalog catalog = RecipeBookCatalog.Find();

        if (catalog == null)
            return null;

        return FindRecipe(catalog, recipeName);
    }

    static RecipeData FindRecipe(
        RecipeBookCatalog catalog,
        string recipeName)
    {
        if (catalog == null ||
            catalog.recipes == null ||
            string.IsNullOrEmpty(recipeName))
        {
            return null;
        }

        foreach (RecipeData recipe in catalog.recipes)
        {
            if (recipe != null &&
                recipe.recipeName == recipeName)
            {
                return recipe;
            }
        }

        return null;
    }

    public bool IsUnlocked(RecipeData recipe)
    {
        if (recipe == null)
            return false;

        if (recipe.unlockedByDefault ||
            recipe.unlockCost <= 0)
        {
            return true;
        }

        return unlockedRecipes.Contains(recipe);
    }

    public bool UnlockRecipe(RecipeData recipe)
    {
        if (recipe == null)
            return false;

        if (IsUnlocked(recipe))
        {
            NotificationManager.Instance.ShowMessage(
                recipe.recipeName +
                " is already unlocked!"
            );

            return true;
        }

        if (CurrencyManager.Instance == null)
        {
            Debug.LogError(
                "[RECIPE] CurrencyManager missing!"
            );

            return false;
        }

        if (CurrencyManager.Instance.SpendCoins(
            recipe.unlockCost))
        {
            unlockedRecipes.Add(recipe);

            NotificationManager.Instance.ShowMessage(
                recipe.recipeName +
                " Unlocked!"
            );

            Debug.Log(
                "[RECIPE] Unlocked: " +
                recipe.recipeName
            );

            return true;
        }

        NotificationManager.Instance.ShowMessage(
            "Not Enough Coins!"
        );

        return false;
    }
}
