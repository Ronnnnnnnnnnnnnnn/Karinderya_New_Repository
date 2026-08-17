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

        // Add starting recipes
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
    }

    // =====================================================
    // CHECK UNLOCK
    // =====================================================

    public bool IsUnlocked(RecipeData recipe)
    {
        if (recipe == null)
            return false;

        // Free recipes are automatically unlocked
        if (recipe.unlockCost <= 0)
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