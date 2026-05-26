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

    float timer;

    bool served;

    [Header("UI")]
    public TextMeshPro orderText;

    public SpriteRenderer orderSprite;

    [Header("Angry")]
    public Color angryColor = Color.red;

    Renderer rend;

    CustomerAI ai;

    void Start()
    {
        timer = patienceTime;

        ai = GetComponent<CustomerAI>();

        rend = GetComponentInChildren<Renderer>();

        UpdateVisuals();

        Debug.Log(
            "[CUSTOMER] Waiting for: " +
            requestedServing.itemName
        );
    }

    void Update()
    {
        if(served)
            return;

        timer -= Time.deltaTime;

        UpdateTimerUI();

        if(timer <= 0)
        {
            LeaveAngry();
        }
    }

    // =====================================================
    // SET ORDER
    // =====================================================

    public void SetOrder(ItemData serving)
    {
        requestedServing = serving;

        UpdateVisuals();

        Debug.Log(
            "[CUSTOMER] Ordered: " +
            serving.itemName
        );
    }

    // =====================================================
    // SERVE CUSTOMER
    // =====================================================

    public void TryServe()
    {
        Debug.Log("[CUSTOMER] TryServe");

        if(served)
            return;

        // PLAYER MUST HOLD ITEM
        if(!InventoryManager.Instance.SlotEquipped(
            InventorySlot.InventoryType.Item))
        {
            Debug.Log("[CUSTOMER] No held item");

            return;
        }

        ItemData heldItem =
            InventoryManager.Instance.GetEquippedSlotItem(
                InventorySlot.InventoryType.Item
            );

        if(heldItem == null)
        {
            Debug.Log("[CUSTOMER] Held item null");

            return;
        }

        Debug.Log(
            "[CUSTOMER] Player holding: " +
            heldItem.itemName
        );

        Debug.Log(
            "[CUSTOMER] Wants: " +
            requestedServing.itemName
        );

        // WRONG FOOD
        if(heldItem != requestedServing)
        {
            Debug.Log("[CUSTOMER] Wrong order");

            return;
        }

        // REMOVE ITEM
        InventoryManager.Instance.ConsumeItem(
            InventoryManager.Instance.GetEquippedSlot(
                InventorySlot.InventoryType.Item
            )
        );

        served = true;

        // GIVE COINS
        if(CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.AddCoins(
                rewardCoins
            );

            Debug.Log(
                "[CUSTOMER] Rewarded Coins: " +
                rewardCoins
            );
        }

        Debug.Log(
            "[CUSTOMER] Correct order served"
        );

        // HAPPY LEAVE
        if(ai != null)
        {
            ai.LeaveHappy();
        }
    }

    // =====================================================
    // LEAVE ANGRY
    // =====================================================

    void LeaveAngry()
    {
        Debug.Log("[CUSTOMER] LEFT ANGRY");

        if(rend != null)
        {
            rend.material.color = angryColor;
        }

        if(ai != null)
        {
            ai.LeaveAngry();
        }

        Destroy(this);
    }

    // =====================================================
    // VISUALS
    // =====================================================

    void UpdateVisuals()
    {
        if(orderText != null)
        {
            orderText.text =
                requestedServing.itemName;
        }

        if(orderSprite != null)
        {
            orderSprite.sprite =
                requestedServing.itemSprite;
        }
    }

    void UpdateTimerUI()
    {
        if(orderText != null)
        {
            orderText.text =
                requestedServing.itemName +
                "\n" +
                Mathf.Ceil(timer).ToString() +
                "s";
        }
    }
}