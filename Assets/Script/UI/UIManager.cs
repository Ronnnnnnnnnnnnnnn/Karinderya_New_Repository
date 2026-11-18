using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 

public class UIManager : MonoBehaviour, ITimeTracker
{
    public static UIManager Instance { get; private set; }
    [Header("Status Bar")]
    public Image toolEquipSlot;
    public Text toolQuantityText; 
    public Text timeText;
    public Text dateText; 


    [Header("Inventory System")]
    public GameObject inventoryPanel;

    public HandInventorySlot toolHandSlot; 

    public InventorySlot[] toolSlots;

    public HandInventorySlot itemHandSlot;

    public InventorySlot[] itemSlots;
   
    public Text itemNameText;
    public Text itemDescriptionText; 

    [Header("Currency")]
    public Text coinText;

    [Header("Shop")]
    public GameObject shopPanel;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        RenderInventory();
        AssignSlotIndexes();
        EnsureTimeDisplay();

        TimeManager.Instance.RegisterTracker(this);

        UpdateCoinUI(CurrencyManager.Instance.coins);
    }

    void EnsureTimeDisplay()
    {
        Canvas canvas = UIRoot.FindGameCanvas();

        if (canvas == null)
            return;

        // Reuse the project's existing Time Info object instead of
        // creating a second clock. It is a child of the Status Bar and
        // starts disabled, so move it to the HUD and make it visible.
        if (timeText != null && dateText != null)
        {
            Transform root = timeText.transform.parent;

            if (root != null)
            {
                root.SetParent(canvas.transform, false);
                root.gameObject.SetActive(true);

                RectTransform panelRect = root as RectTransform;

                if (panelRect != null)
                {
                    panelRect.anchorMin = new Vector2(0.5f, 1f);
                    panelRect.anchorMax = new Vector2(0.5f, 1f);
                    panelRect.pivot = new Vector2(0.5f, 1f);
                    panelRect.anchoredPosition = new Vector2(0f, -8f);
                    panelRect.sizeDelta = new Vector2(320f, 100f);
                }

                Image background = root.GetComponent<Image>();

                if (background == null)
                    background = root.gameObject.AddComponent<Image>();

                background.color = new Color(0.08f, 0.06f, 0.04f, 0.84f);
                background.raycastTarget = false;

                StyleTimeText(
                    timeText,
                    34,
                    -4f,
                    44f,
                    new Color(1f, 0.96f, 0.88f)
                );

                timeText.fontStyle = FontStyle.Bold;

                StyleTimeText(
                    dateText,
                    22,
                    -52f,
                    30f,
                    new Color(0.94f, 0.88f, 0.72f)
                );

                dateText.fontStyle = FontStyle.Normal;

                timeText.gameObject.SetActive(true);
                dateText.gameObject.SetActive(true);

                Transform weather = root.Find("Weather");
                if (weather != null)
                    weather.gameObject.SetActive(false);

                return;
            }
        }

        // Fallback if a scene has no Time Info objects.
        GameObject panel =
            new GameObject("TimeDisplay", typeof(RectTransform));

        panel.transform.SetParent(canvas.transform, false);

        RectTransform fallbackRect = panel.GetComponent<RectTransform>();
        fallbackRect.anchorMin = new Vector2(0.5f, 1f);
        fallbackRect.anchorMax = new Vector2(0.5f, 1f);
        fallbackRect.pivot = new Vector2(0.5f, 1f);
        fallbackRect.anchoredPosition = new Vector2(0f, -8f);
        fallbackRect.sizeDelta = new Vector2(320f, 100f);

        Image fallbackBackground = panel.AddComponent<Image>();
        fallbackBackground.color =
            new Color(0.08f, 0.06f, 0.04f, 0.84f);
        fallbackBackground.raycastTarget = false;

        timeText = CreateHudText(
            panel.transform,
            "Time",
            34,
            TextAnchor.MiddleCenter,
            new Vector2(0f, -4f),
            new Vector2(300f, 44f),
            new Color(1f, 0.96f, 0.88f)
        );

        timeText.fontStyle = FontStyle.Bold;

        dateText = CreateHudText(
            panel.transform,
            "Date",
            22,
            TextAnchor.MiddleCenter,
            new Vector2(0f, -52f),
            new Vector2(300f, 30f),
            new Color(0.94f, 0.88f, 0.72f)
        );
    }

    static void StyleTimeText(
        Text text,
        int fontSize,
        float anchoredY,
        float height,
        Color color)
    {
        RectTransform rect = text.rectTransform;
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, anchoredY);
        rect.sizeDelta = new Vector2(300f, height);

        text.fontSize = fontSize;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = color;
        text.raycastTarget = false;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;

        Shadow shadow = text.GetComponent<Shadow>();

        if (shadow == null)
            shadow = text.gameObject.AddComponent<Shadow>();

        shadow.effectColor = new Color(0f, 0f, 0f, 0.85f);
        shadow.effectDistance = new Vector2(1f, -1f);
    }

    static Text CreateHudText(
        Transform parent,
        string name,
        int fontSize,
        TextAnchor alignment,
        Vector2 anchoredPosition,
        Vector2 rectSize,
        Color color)
    {
        GameObject go =
            new GameObject(name, typeof(RectTransform));

        go.transform.SetParent(parent, false);

        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = rectSize;

        Text text = go.AddComponent<Text>();
        text.font =
            Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = color;
        text.raycastTarget = false;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;

        Shadow shadow = go.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.85f);
        shadow.effectDistance = new Vector2(1f, -1f);

        return text;
    }

    public void AssignSlotIndexes()
    {
        for (int i =0; i<toolSlots.Length; i++)
        {
            toolSlots[i].AssignIndex(i);
            itemSlots[i].AssignIndex(i);
        }
    }

    public void RenderInventory()
    {
        ItemSlotData[] inventoryToolSlots = InventoryManager.Instance.GetInventorySlots(InventorySlot.InventoryType.Tool);
        ItemSlotData[] inventoryItemSlots = InventoryManager.Instance.GetInventorySlots(InventorySlot.InventoryType.Item);

        RenderInventoryPanel(inventoryToolSlots, toolSlots);

        RenderInventoryPanel(inventoryItemSlots, itemSlots);

        toolHandSlot.Display(InventoryManager.Instance.GetEquippedSlot(InventorySlot.InventoryType.Tool));
        itemHandSlot.Display(InventoryManager.Instance.GetEquippedSlot(InventorySlot.InventoryType.Item));

        ItemData equippedTool = InventoryManager.Instance.GetEquippedSlotItem(InventorySlot.InventoryType.Tool);

        toolQuantityText.text = "";
        if (equippedTool != null)
        {
            toolEquipSlot.sprite = equippedTool.thumbnail;

            toolEquipSlot.gameObject.SetActive(true);

            int quantity = InventoryManager.Instance.GetEquippedSlot(InventorySlot.InventoryType.Tool).quantity;
            if (quantity > 1)
            {
                toolQuantityText.text = quantity.ToString();
            }
            return;
        }

        toolEquipSlot.gameObject.SetActive(false);
    }

    void RenderInventoryPanel(ItemSlotData[] slots, InventorySlot[] uiSlots)
    {
        for (int i = 0; i < uiSlots.Length; i++)
        {
            uiSlots[i].Display(slots[i]);
        }
    }

    public bool IsMenuOpen =>
        (inventoryPanel != null && inventoryPanel.activeSelf) ||
        (shopPanel != null && shopPanel.activeSelf);

    public void ToggleInventoryPanel()
    {
        bool open = !inventoryPanel.activeSelf;

        if (open && shopPanel != null)
            shopPanel.SetActive(false);

        inventoryPanel.SetActive(open);
        RenderInventory();
        RefreshCursorState();
    }

    public void ToggleShop()
    {
        bool open = !shopPanel.activeSelf;

        if (open && inventoryPanel != null)
            inventoryPanel.SetActive(false);

        shopPanel.SetActive(open);
        RefreshCursorState();
    }

    public void RefreshCursorState()
    {
        CameraMovement cameraMovement =
            FindFirstObjectByType<CameraMovement>();
        if (cameraMovement != null)
            cameraMovement.RefreshCursorState();
    }

    public void DisplayItemInfo(ItemData data)
    {
        if(data == null)
        {
            itemNameText.text = "";
            itemDescriptionText.text = "";

            return;
        }

        itemNameText.text = GetItemDisplayName(data);
        itemDescriptionText.text = data.description; 
    }

    static string GetItemDisplayName(ItemData data)
    {
        if (!string.IsNullOrEmpty(data.itemName))
            return data.itemName;

        return data.name;
    }

    public void ClockUpdate(GameTimestamp timestamp)
    {
        int hours = timestamp.hour;
        int minutes = timestamp.minute; 

        string prefix = hours >= 12 ? "PM " : "AM ";

        hours = hours % 12;
        if (hours == 0)
            hours = 12;

        timeText.text = prefix + hours + ":" + minutes.ToString("00");

        int day = timestamp.day;
        string season = timestamp.season.ToString();
        string dayOfTheWeek = timestamp.GetDayOfTheWeek().ToString();

        dateText.text = season + " " + day + " (" + dayOfTheWeek +")";

    }

    public void UpdateCoinUI(int amount)
    {
        coinText.text = "₱ " + amount.ToString();
    }

    void Update()
    {
        if (timeText == null)
            return;

        Transform timeRoot = timeText.transform.parent;

        if (timeRoot == null)
            return;

        bool hide =
            PauseMenu.Pause ||
            IsMenuOpen ||
            (CookingUIManager.Instance != null &&
             CookingUIManager.Instance.IsOpen);

        if (timeRoot.gameObject.activeSelf == hide)
            timeRoot.gameObject.SetActive(!hide);
    }
}