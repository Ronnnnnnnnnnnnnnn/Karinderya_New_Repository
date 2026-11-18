using System;
using TMPro;
using UnityEngine;

public class CustomerOrder : MonoBehaviour
{
    // Fired when a customer is finished, either served or angry.
    // The tutorial uses this to advance without polling private state.
    public static event Action<CustomerOrder, bool> CustomerFinished;

    [Header("Order")]
    public ItemData requestedServing;

    [Header("Reward")]
    public int rewardCoins = 60;

    [Header("Patience")]
    public float patienceTime = 60f;

    [Tooltip(
        "Percentage of reward deducted when customer leaves."
    )]
    [Range(0f, 1f)]
    public float compensationRate = 0.5f;

    float timer;

    bool served;

    bool waitingStarted;

    [Header("UI")]
    public TextMeshPro orderText;

    public SpriteRenderer orderSprite;

    [Header("Angry")]
    public Color angryColor = Color.red;

    Renderer[] renderers;

    CustomerAI ai;

    void Start()
    {
        ai = GetComponent<CustomerAI>();

        renderers =
            GetComponentsInChildren<Renderer>();

        UpdateVisuals();

        Debug.Log("[CUSTOMER] Spawned");
    }

    void Update()
    {
        if (served) return;

        if (!waitingStarted) return;

        timer -= Time.deltaTime;

        UpdateTimerUI();

        if (timer <= 0)
        {
            LeaveAngry();
        }
    }

    public void StartWaiting()
    {
        waitingStarted = true;

        timer = patienceTime;
    }

    public void SetOrder(ItemData serving)
    {
        requestedServing = serving;

        RecipeData recipe =
            FindRecipeForDish(serving);

        if (recipe != null)
        {
            rewardCoins = recipe.sellingPrice;
        }

        UpdateVisuals();

        Debug.Log(
            "[CUSTOMER] Ordered: " +
            serving.itemName +
            " | Reward: ₱" +
            rewardCoins
        );
    }

    public void TryServe()
    {
        if (served) return;

        if (!waitingStarted) return;

        if (!InventoryManager.Instance.SlotEquipped(
            InventorySlot.InventoryType.Item))
        {
            NotificationManager.Instance.ShowMessage(
                "Hold A Dish!"
            );

            // 🔊 ERROR SOUND
            PlayErrorSound();

            return;
        }

        ItemData heldItem =
            InventoryManager.Instance.GetEquippedSlotItem(
                InventorySlot.InventoryType.Item
            );

        if (heldItem == null)
        {
            return;
        }

        if (heldItem != requestedServing)
        {
            NotificationManager.Instance.ShowMessage(
                "Wrong Dish!"
            );

            // 🔊 WRONG DISH SOUND
            PlayErrorSound();

            return;
        }

        // Correct dish
        InventoryManager.Instance.ConsumeItem(
            InventoryManager.Instance.GetEquippedSlot(
                InventorySlot.InventoryType.Item
            )
        );

        served = true;

        CurrencyManager.Instance.AddCoins(
            rewardCoins
        );

        if (PlayerProgression.Instance != null)
        {
            PlayerProgression.Instance.OnDishServed(
                rewardCoins
            );
        }

        NotificationManager.Instance.ShowMessage(
            "+" + rewardCoins + " Coins!"
        );

        Debug.Log(
            "[CUSTOMER] Served correctly"
        );

        // 🔊 CUSTOMER SERVED SOUND
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(
                AudioManager.Instance.serveSound
            );
        }

        if (ai != null)
        {
            ai.LeaveHappy();
        }

        CustomerFinished?.Invoke(this, true);
    }

    void LeaveAngry()
    {
        served = true;

        NotificationManager.Instance.ShowMessage(
            "Customer Angry!"
        );

        int compensation =
            Mathf.RoundToInt(
                rewardCoins * compensationRate
            );

        CurrencyManager.Instance.AddCoins(
            -compensation
        );

        NotificationManager.Instance.ShowMessage(
            "-" +
            compensation +
            " Coins (Compensation)"
        );

        MaterialPropertyBlock angryBlock =
            new MaterialPropertyBlock();

        foreach (Renderer rend in renderers)
        {
            if (rend != null)
            {
                rend.GetPropertyBlock(angryBlock);
                angryBlock.SetColor("_BaseColor", angryColor);
                angryBlock.SetColor("_Color", angryColor);
                rend.SetPropertyBlock(angryBlock);
            }
        }

        // 🔊 CUSTOMER ANGRY / ERROR SOUND
        PlayErrorSound();

        if (ai != null)
        {
            ai.LeaveAngry();
        }

        CustomerFinished?.Invoke(this, false);

        enabled = false;
    }

    void PlayErrorSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(
                AudioManager.Instance.errorSound
            );
        }
    }

    RecipeData FindRecipeForDish(ItemData dish)
    {
        if (dish == null)
        {
            return null;
        }

        RecipeBookCatalog catalog =
            RecipeBookCatalog.Find();

        if (catalog == null ||
            catalog.recipes == null)
        {
            return null;
        }

        foreach (RecipeData recipe in catalog.recipes)
        {
            if (recipe == null ||
                recipe.resultDish == null)
            {
                continue;
            }

            if (recipe.resultDish == dish ||
                recipe.resultDish.servingVersion == dish)
            {
                return recipe;
            }
        }

        return null;
    }

    void UpdateVisuals()
    {
        if (requestedServing == null)
        {
            return;
        }

        if (orderText != null)
        {
            orderText.text =
                requestedServing.itemName;
        }

        if (orderSprite != null)
        {
            orderSprite.sprite =
                requestedServing.itemSprite;
        }
    }

    void UpdateTimerUI()
    {
        if (orderText != null &&
            requestedServing != null)
        {
            orderText.text =
                requestedServing.itemName +
                "\n" +
                Mathf.Ceil(timer).ToString() +
                "s";
        }
    }
}