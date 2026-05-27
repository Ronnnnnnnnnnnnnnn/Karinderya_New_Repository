using System.Collections;
using UnityEngine;
using TMPro;

public class Pot : MonoBehaviour
{
    [Header("Recipe")]
    public RecipeData recipe;

    [Header("Cooking")]
    public float cookTime = 10f;

    bool cooking;

    bool cooked;

    // =========================================
    // VISUALS
    // =========================================

    [Header("Dish Visual")]
    public SpriteRenderer dishRenderer;

    [Header("Timer Visual")]
    public SpriteRenderer timerRenderer;

    [Header("Timer Sprites")]
    public Sprite[] numberSprites;

    public Sprite cookedSprite;

    // =========================================

    int currentIngredientCount;

    ItemData cookedDish;

    void Start()
    {
        HideDish();

        HideTimer();
    }

    // =========================================
    // INTERACT
    // =========================================

    public void Interact()
    {
        // TAKE FINISHED DISH
        if(cooked)
        {
            GiveDish();

            return;
        }

        // ALREADY COOKING
        if(cooking)
            return;

        AddIngredient();
    }

    // =========================================
    // ADD INGREDIENT
    // =========================================

    void AddIngredient()
    {
        if(!InventoryManager.Instance.SlotEquipped(
            InventorySlot.InventoryType.Item))
        {
            return;
        }

        ItemData heldItem =
            InventoryManager.Instance.GetEquippedSlotItem(
                InventorySlot.InventoryType.Item
            );

        if(heldItem == null)
            return;

        // CHECK RECIPE
        foreach(ItemData ingredient in recipe.ingredients)
        {
            if(heldItem == ingredient)
            {
                currentIngredientCount++;

                InventoryManager.Instance.ConsumeItem(
                    InventoryManager.Instance.GetEquippedSlot(
                        InventorySlot.InventoryType.Item
                    )
                );

                NotificationManager.Instance.ShowMessage(
                    ingredient.itemName + " Added"
                );

                Debug.Log(
                    "[POT] Ingredient Added"
                );

                break;
            }
        }

        // START COOKING
        if(currentIngredientCount >=
            recipe.ingredients.Length)
        {
            StartCoroutine(CookRoutine());
        }
    }

    // =========================================
    // COOKING
    // =========================================

    IEnumerator CookRoutine()
    {
        cooking = true;

        float timer = cookTime;

        ShowTimer();

        while(timer > 0)
        {
            int rounded =
                Mathf.CeilToInt(timer);

            UpdateTimerSprite(rounded);

            yield return new WaitForSeconds(1f);

            timer--;
        }

        FinishCooking();
    }

    // =========================================
    // FINISH
    // =========================================

    void FinishCooking()
    {
        cooking = false;

        cooked = true;

        cookedDish = recipe.resultDish;

        HideTimer();

        ShowDish();

        NotificationManager.Instance.ShowMessage(
            recipe.resultDish.itemName +
            " Cooked!"
        );

        Debug.Log(
            "[POT] Finished Cooking"
        );
    }

    // =========================================
    // GIVE DISH
    // =========================================

    void GiveDish()
    {
        InventoryManager.Instance.AddItem(
            cookedDish
        );

        NotificationManager.Instance.ShowMessage(
            cookedDish.itemName +
            " Taken"
        );

        cooked = false;

        currentIngredientCount = 0;

        cookedDish = null;

        HideDish();
    }

    // =========================================
    // DISH VISUAL
    // =========================================

    void ShowDish()
    {
        if(dishRenderer == null)
            return;

        dishRenderer.sprite =
            recipe.resultDish.itemSprite;

        dishRenderer.gameObject.SetActive(true);
    }

    void HideDish()
    {
        if(dishRenderer == null)
            return;

        dishRenderer.gameObject.SetActive(false);
    }

    // =========================================
    // TIMER VISUAL
    // =========================================

    void ShowTimer()
    {
        if(timerRenderer == null)
            return;

        timerRenderer.gameObject.SetActive(true);
    }

    void HideTimer()
    {
        if(timerRenderer == null)
            return;

        timerRenderer.gameObject.SetActive(false);
    }

    void UpdateTimerSprite(int number)
    {
        if(timerRenderer == null)
            return;

        if(number < 0 ||
            number >= numberSprites.Length)
        {
            return;
        }

        timerRenderer.sprite =
            numberSprites[number];
    }
}