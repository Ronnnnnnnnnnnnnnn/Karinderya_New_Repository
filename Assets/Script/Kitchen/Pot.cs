using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Pot : MonoBehaviour
{
    [Header("Recipes")]
    public List<RecipeData> recipes;

    [Header("Inserted Ingredients")]
    public List<ItemData> insertedIngredients =
        new List<ItemData>();


    [Header("Cooking")]
    public bool isCooking;

    public bool foodReady;

    public float timer;

    RecipeData currentRecipe;

    ItemData cookedDish;

    [Header("Visuals")]
    public SpriteRenderer dishRenderer;

    public TextMeshPro ingredientText;

    public TextMeshPro timerText;

    void Update()
    {
        UpdateVisuals();

        if(isCooking)
        {
            timer -= Time.deltaTime;

            timerText.text =
                Mathf.Ceil(timer).ToString();

            if(timer <= 0)
            {
                FinishCooking();
            }
        }
    }

    public void Interact()
    {
        Debug.Log("[POT] Interacted");

        // TAKE COOKED FOOD
        if(foodReady)
        {
            GiveCookedDish();
            return;
        }

        // INSERT INGREDIENT
        AddIngredient();
    }

    void AddIngredient()
    {
        if(!InventoryManager.Instance.SlotEquipped(
            InventorySlot.InventoryType.Item))
        {
            Debug.Log("[POT] No item equipped");
            return;
        }

        ItemData heldItem =
            InventoryManager.Instance.GetEquippedSlotItem(
                InventorySlot.InventoryType.Item
            );

        if(heldItem == null)
            return;

        if(!heldItem.isIngredient)
        {
            Debug.Log("[POT] Not ingredient");
            return;
        }

        insertedIngredients.Add(heldItem);

        Debug.Log(
            "[POT] Added ingredient: " +
            heldItem.itemName
        );

        InventoryManager.Instance.ConsumeItem(
            InventoryManager.Instance.GetEquippedSlot(
                InventorySlot.InventoryType.Item
            )
        );

        CheckRecipe();
    }

    void CheckRecipe()
    {
        foreach(RecipeData recipe in recipes)
        {
            if(MatchesRecipe(recipe))
            {
                StartCooking(recipe);
                return;
            }
        }
    }

    bool MatchesRecipe(RecipeData recipe)
    {
        if(insertedIngredients.Count !=
            recipe.requiredIngredients.Count)
        {
            return false;
        }

        foreach(ItemData ingredient
            in recipe.requiredIngredients)
        {
            if(!insertedIngredients.Contains(
                ingredient))
            {
                return false;
            }
        }

        return true;
    }

    void StartCooking(RecipeData recipe)
    {
        currentRecipe = recipe;

        isCooking = true;

        timer = recipe.cookTime;

        Debug.Log(
            "[POT] Started cooking: " +
            recipe.dishName
        );

        if(dishRenderer != null)
        {
            dishRenderer.sprite =
                recipe.dishSprite;
        }
    }

    void FinishCooking()
    {
        isCooking = false;

        foodReady = true;

        cookedDish =
            currentRecipe.cookedDish;

        Debug.Log(
            "[POT] Finished cooking: " +
            cookedDish.itemName
        );
    }

    void GiveCookedDish()
    {
        if(cookedDish == null)
            return;

        InventoryManager.Instance.AddItem(
            cookedDish
        );

        Debug.Log(
            "[POT] Gave dish: " +
            cookedDish.itemName
        );

        insertedIngredients.Clear();

        currentRecipe = null;

        cookedDish = null;

        foodReady = false;

        timer = 0;

        if(dishRenderer != null)
        {
            dishRenderer.sprite = null;
        }
    }

    void UpdateVisuals()
    {
        ingredientText.text =
            insertedIngredients.Count +
            "/4 Ingredients";
    }
}