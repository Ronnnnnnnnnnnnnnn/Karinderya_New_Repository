using System;
using TMPro;
using UnityEngine;

public class CustomerOrder : MonoBehaviour
{

    public static event Action<CustomerOrder, bool> CustomerFinished;

    [Header("Order")]
    public ItemData requestedServing;

    [Header("Reward")]
    public int rewardCoins = 60;

    [HideInInspector]
    public bool isRushCustomer;

    [HideInInspector]
    public bool isRegular;

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

    static readonly Color LabelNormalColor =
        new Color(1f, 0.96f, 0.85f, 1f);
    static readonly Color LabelWarningColor =
        new Color(1f, 0.78f, 0.25f, 1f);
    static readonly Color LabelCriticalColor =
        new Color(1f, 0.35f, 0.28f, 1f);

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

    public void ConfigureLabel(int slotIndex)
    {
        if (orderText == null)
            return;

        RectTransform rect = orderText.rectTransform;

        rect.sizeDelta = new Vector2(16f, 1.2f);
        rect.anchoredPosition = new Vector2(
            0f,
            4.85f + Mathf.Max(0, slotIndex) * 1.15f
        );

        orderText.fontSize = 0.9f;
        orderText.fontStyle = FontStyles.Bold;
        orderText.textWrappingMode = TextWrappingModes.NoWrap;
        orderText.overflowMode = TextOverflowModes.Overflow;
        orderText.outlineWidth = 0.15f;
        orderText.outlineColor =
            new Color(0.04f, 0.03f, 0.02f, 1f);
        orderText.color = LabelNormalColor;
    }

    public void HideOrderVisuals()
    {
        if (orderText != null)
            orderText.gameObject.SetActive(false);

        if (orderSprite != null)
            orderSprite.gameObject.SetActive(false);
    }

    public void SetRegular()
    {
        isRegular = true;
        UpdateVisuals();
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

            PlayErrorSound();

            return;
        }

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
            "+" + rewardCoins + " Coins!" +
            (isRushCustomer ? " (Rush)" : "") +
            (isRegular ? " (Regular)" : "")
        );

        Debug.Log(
            "[CUSTOMER] Served correctly"
        );

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
                (isRegular ? "* " : "") +
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
                (isRegular ? "* " : "") +
                requestedServing.itemName +
                "  " +
                Mathf.Ceil(timer).ToString() +
                "s";

            float patienceLeft =
                patienceTime > 0f
                    ? timer / patienceTime
                    : 0f;

            if (patienceLeft <= 0.25f)
            {
                orderText.color = LabelCriticalColor;
            }
            else if (patienceLeft <= 0.5f)
            {
                orderText.color = LabelWarningColor;
            }
            else
            {
                orderText.color = LabelNormalColor;
            }
        }
    }
}
