using TMPro;
using UnityEngine;

public class CustomerOrder : MonoBehaviour
{
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

    // =====================================================
    // START
    // =====================================================

    void Start()
    {
        ai =
            GetComponent<CustomerAI>();

        renderers =
            GetComponentsInChildren<Renderer>();

        UpdateVisuals();

        Debug.Log(
            "[CUSTOMER] Spawned"
        );
    }

    // =====================================================
    // UPDATE
    // =====================================================

    void Update()
    {
        if (served)
            return;

        if (!waitingStarted)
            return;

        timer -=
            Time.deltaTime;

        UpdateTimerUI();

        if (timer <= 0)
        {
            LeaveAngry();
        }
    }

    // =====================================================
    // START WAITING
    // =====================================================

    public void StartWaiting()
    {
        waitingStarted = true;

        timer =
            patienceTime;

        NotificationManager.Instance.ShowMessage(
            "Customer Waiting!"
        );
    }

    // =====================================================
    // SET ORDER
    // =====================================================

    public void SetOrder(
        ItemData serving)
    {
        requestedServing =
            serving;

        // Automatically determine selling price
        RecipeData recipe =
            FindRecipeForDish(
                serving
            );

        if (recipe != null)
        {
            rewardCoins =
                recipe.sellingPrice;
        }

        UpdateVisuals();

        Debug.Log(
            "[CUSTOMER] Ordered: " +
            serving.itemName +
            " | Reward: ₱" +
            rewardCoins
        );
    }

    // =====================================================
    // SERVE
    // =====================================================

    public void TryServe()
    {
        if (served)
            return;

        if (!waitingStarted)
            return;

        if (!InventoryManager.Instance.SlotEquipped(
            InventorySlot.InventoryType.Item))
        {
            NotificationManager.Instance.ShowMessage(
                "Hold A Dish!"
            );

            return;
        }

        ItemData heldItem =
            InventoryManager.Instance.GetEquippedSlotItem(
                InventorySlot.InventoryType.Item
            );

        if (heldItem == null)
            return;

        // WRONG FOOD
        if (heldItem != requestedServing)
        {
            NotificationManager.Instance.ShowMessage(
                "Wrong Dish!"
            );

            return;
        }

        // REMOVE FOOD
        InventoryManager.Instance.ConsumeItem(
            InventoryManager.Instance.GetEquippedSlot(
                InventorySlot.InventoryType.Item
            )
        );

        served = true;

        // PAY PLAYER
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
            "+" +
            rewardCoins +
            " Coins!"
        );

        Debug.Log(
            "[CUSTOMER] Served correctly"
        );

        if (ai != null)
        {
            ai.LeaveHappy();
        }
    }

    // =====================================================
    // ANGRY
    // =====================================================

    void LeaveAngry()
    {
        NotificationManager.Instance.ShowMessage(
            "Customer Angry!"
        );

        int compensation =
            Mathf.RoundToInt(
                rewardCoins *
                compensationRate
            );

        CurrencyManager.Instance.AddCoins(
            -compensation
        );

        NotificationManager.Instance.ShowMessage(
            "-" +
            compensation +
            " Coins (Compensation)"
        );

        foreach (
            Renderer rend in renderers)
        {
            if (rend != null)
            {
                rend.material.color =
                    angryColor;
            }
        }

        if (ai != null)
        {
            ai.LeaveAngry();
        }

        enabled = false;
    }

    // =====================================================
    // FIND RECIPE
    // =====================================================

    RecipeData FindRecipeForDish(
        ItemData dish)
    {
        if (dish == null)
            return null;

        RecipeData[] recipes =
            Resources.FindObjectsOfTypeAll<RecipeData>();

        foreach (RecipeData recipe in recipes)
        {
            if (recipe == null)
                continue;

            if (recipe.resultDish == dish)
            {
                return recipe;
            }
        }

        return null;
    }

    // =====================================================
    // VISUALS
    // =====================================================

    void UpdateVisuals()
    {
        if (requestedServing == null)
            return;

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

    // =====================================================
    // TIMER
    // =====================================================

    void UpdateTimerUI()
    {
        if (orderText != null &&
            requestedServing != null)
        {
            orderText.text =
                requestedServing.itemName +
                "\n" +
                Mathf.Ceil(
                    timer
                ).ToString() +
                "s";
        }
    }
}