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

    [Header("Dish Visual")]
    public SpriteRenderer dishRenderer;

    [Header("Timer Visual")]
    public SpriteRenderer timerRenderer;

    [Header("Timer Sprites")]
    public Sprite[] numberSprites;

    public Sprite cookedSprite;

    [Header("Ingredient Counter")]
    public TextMeshPro ingredientText;

    int currentIngredientCount;

    ItemData cookedDish;

    void Start()
    {
        HideDish();
        HideTimer();
        UpdateIngredientText();
    }

    public void Interact()
    {
        if(cooked)
        {
            GiveDish();
            return;
        }

        if(cooking)
            return;

        AddIngredient();
    }

    void AddIngredient()
    {
        if(recipe == null)
        {
            NotificationManager.Instance.ShowMessage(
                "No Recipe Assigned!"
            );

            Debug.LogError(
                "[POT] No Recipe Assigned!"
            );

            return;
        }

        if(!InventoryManager.Instance.SlotEquipped(
            InventorySlot.InventoryType.Item))
        {
            NotificationManager.Instance.ShowMessage(
                "Hold an ingredient first!"
            );

            return;
        }

        ItemData heldItem =
            InventoryManager.Instance.GetEquippedSlotItem(
                InventorySlot.InventoryType.Item
            );

        if(heldItem == null)
            return;

        bool ingredientAccepted = false;

        foreach(ItemData ingredient in recipe.ingredients)
        {
            if(heldItem == ingredient)
            {
                ingredientAccepted = true;

                currentIngredientCount++;

                UpdateIngredientText();

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

        if(!ingredientAccepted)
        {
            NotificationManager.Instance.ShowMessage(
                "Wrong Ingredient!"
            );

            return;
        }

        if(currentIngredientCount >= recipe.ingredients.Length)
        {
            StartCoroutine(CookRoutine());
        }
    }

    IEnumerator CookRoutine()
    {
        cooking = true;

        if(ingredientText != null)
        {
            ingredientText.gameObject.SetActive(false);
        }

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

        UpdateIngredientText();

        if(ingredientText != null)
        {
            ingredientText.gameObject.SetActive(true);
        }
    }

    void ShowDish()
    {
        if(dishRenderer == null)
            return;

        if(recipe == null)
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

    void UpdateIngredientText()
    {
        if(ingredientText == null)
            return;

        if(recipe == null)
        {
            ingredientText.text = "NO RECIPE";
            return;
        }

        if(recipe.ingredients == null)
        {
            ingredientText.text = "NO INGREDIENTS";
            return;
        }

        ingredientText.text =
            currentIngredientCount +
            "/" +
            recipe.ingredients.Length;
    }
}