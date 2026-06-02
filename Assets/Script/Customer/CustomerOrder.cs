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

        // GET ALL RENDERERS SAFELY
        renderers =
            GetComponentsInChildren<Renderer>();

        UpdateVisuals();

        Debug.Log(
            "[CUSTOMER] Spawned"
        );
    }

    void Update()
    {
        if(served)
            return;

        if(!waitingStarted)
            return;

        timer -= Time.deltaTime;

        UpdateTimerUI();

        if(timer <= 0)
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

        timer = patienceTime;

        NotificationManager.Instance.ShowMessage(
            "Customer Waiting!"
        );

        Debug.Log(
            "[CUSTOMER] Started waiting"
        );
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
        if(served)
            return;

        if(!waitingStarted)
            return;

        // HOLDING NOTHING
        if(!InventoryManager.Instance.SlotEquipped(
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

        if(heldItem == null)
            return;

        // WRONG FOOD
        if(heldItem != requestedServing)
        {
            NotificationManager.Instance.ShowMessage(
                "Wrong Dish!"
            );

            Debug.Log(
                "[CUSTOMER] Wrong food"
            );

            return;
        }

        // REMOVE ITEM
        InventoryManager.Instance.ConsumeItem(
            InventoryManager.Instance.GetEquippedSlot(
                InventorySlot.InventoryType.Item
            )
        );

        served = true;

        CurrencyManager.Instance.AddCoins(rewardCoins);

        if (PlayerProgression.Instance != null)
            PlayerProgression.Instance.OnDishServed(rewardCoins);

        NotificationManager.Instance.ShowMessage(
            "+" + rewardCoins + " Coins!"
        );

        Debug.Log(
            "[CUSTOMER] Served correctly"
        );

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
        NotificationManager.Instance.ShowMessage(
            "Customer Angry!"
        );

        Debug.Log(
            "[CUSTOMER] LEFT ANGRY"
        );

        // SAFE RENDERER LOOP
        foreach(Renderer rend in renderers)
        {
            if(rend != null)
            {
                rend.material.color =
                    angryColor;
            }
        }

        if(ai != null)
        {
            ai.LeaveAngry();
        }

        enabled = false;
    }

    // =====================================================
    // VISUALS
    // =====================================================

    void UpdateVisuals()
    {
        if(requestedServing == null)
            return;

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

    // =====================================================
    // TIMER UI
    // =====================================================

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