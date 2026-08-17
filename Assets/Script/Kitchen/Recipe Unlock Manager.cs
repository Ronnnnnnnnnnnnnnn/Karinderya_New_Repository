using System.Collections.Generic;
using UnityEngine;

public class RecipeUnlockManager : MonoBehaviour
{
    public static RecipeUnlockManager Instance;

    private HashSet<RecipeData> unlockedRecipes =
        new HashSet<RecipeData>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        InitializeDefaultRecipes();
    }

    // =====================================================
    // INITIALIZE DEFAULT RECIPES
    // =====================================================

    void InitializeDefaultRecipes()
    {
        RecipeData[] recipes =
            Resources.FindObjectsOfTypeAll<RecipeData>();

        foreach (RecipeData recipe in recipes)
        {
            if (recipe == null)
                continue;

            if (recipe.unlockedByDefault)
            {
                unlockedRecipes.Add(recipe);

                Debug.Log(
                    "[UNLOCK] Default unlocked: " +
                    recipe.recipeName
                );
            }
        }
    }

    // =====================================================
    // CHECK UNLOCK
    // =====================================================

    public bool IsUnlocked(RecipeData recipe)
    {
        if (recipe == null)
            return false;

        if (recipe.unlockedByDefault)
            return true;

        return unlockedRecipes.Contains(recipe);
    }

    // =====================================================
    // UNLOCK
    // =====================================================

    public bool UnlockRecipe(RecipeData recipe)
    {
        if (recipe == null)
            return false;

        // Already unlocked
        if (IsUnlocked(recipe))
        {
            NotificationManager.Instance.ShowMessage(
                recipe.recipeName + " Already Unlocked!"
            );

            return true;
        }

        // FREE
        if (recipe.unlockCost <= 0)
        {
            unlockedRecipes.Add(recipe);

            NotificationManager.Instance.ShowMessage(
                recipe.recipeName + " Unlocked!"
            );

            return true;
        }

        // CHECK MONEY
        if (CurrencyManager.Instance == null)
        {
            Debug.LogWarning(
                "[UNLOCK] CurrencyManager missing!"
            );

            return false;
        }

        // TRY TO SPEND
        if (!CurrencyManager.Instance.SpendCoins(
            recipe.unlockCost))
        {
            NotificationManager.Instance.ShowMessage(
                "Not Enough Coins!"
            );

            return false;
        }

        // UNLOCK
        unlockedRecipes.Add(recipe);

        NotificationManager.Instance.ShowMessage(
            recipe.recipeName +
            " Unlocked!"
        );

        Debug.Log(
            "[UNLOCK] " +
            recipe.recipeName +
            " unlocked for ₱" +
            recipe.unlockCost
        );

        return true;
    }
}