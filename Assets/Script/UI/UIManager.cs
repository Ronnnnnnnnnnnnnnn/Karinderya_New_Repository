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
    public Image coinIcon;
    public Sprite coinSprite;

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
        EnsureCurrencyDisplay();

        TimeManager.Instance.RegisterTracker(this);

        if (CurrencyManager.Instance != null)
            UpdateCoinUI(CurrencyManager.Instance.coins);
    }

    void EnsureTimeDisplay()
    {
        Canvas canvas = UIRoot.FindGameCanvas();

        if (canvas == null)
            return;

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
        (shopPanel != null && shopPanel.activeSelf) ||
        MissionNotebookUI.IsOpen;

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

    const float CurrencyPanelWidth = 236f;
    const float CurrencyPanelHeight = 60f;

    static Sprite currencyPanelSprite;
    static Sprite fallbackCoinSprite;

    void EnsureCurrencyDisplay()
    {
        Canvas canvas = UIRoot.FindGameCanvas();

        if (canvas == null)
            return;

        Transform existing = canvas.transform.Find("CurrencyHUD");

        if (existing != null)
        {
            if (coinText == null)
                coinText = existing.GetComponentInChildren<Text>();

            if (coinIcon == null)
            {
                Transform icon = existing.Find("Coin Icon");

                if (icon != null)
                    coinIcon = icon.GetComponent<Image>();
            }

            StyleCoinText();
            return;
        }

        GameObject panel =
            new GameObject("CurrencyHUD", typeof(RectTransform));

        panel.transform.SetParent(canvas.transform, false);

        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0f, 1f);
        panelRect.anchorMax = new Vector2(0f, 1f);
        panelRect.pivot = new Vector2(0f, 1f);
        panelRect.anchoredPosition = new Vector2(18f, -18f);
        panelRect.sizeDelta =
            new Vector2(CurrencyPanelWidth, CurrencyPanelHeight);

        Image background = panel.AddComponent<Image>();
        background.sprite = GetCurrencyPanelSprite();
        background.type = Image.Type.Sliced;
        background.color = new Color(0.08f, 0.06f, 0.04f, 0.86f);
        background.raycastTarget = false;

        if (coinIcon == null)
        {
            GameObject iconGo =
                new GameObject("Coin Icon", typeof(RectTransform));

            iconGo.transform.SetParent(panel.transform, false);
            coinIcon = iconGo.AddComponent<Image>();
        }
        else
        {
            coinIcon.transform.SetParent(panel.transform, false);
        }

        RectTransform iconRect = coinIcon.rectTransform;
        iconRect.anchorMin = new Vector2(0f, 0.5f);
        iconRect.anchorMax = new Vector2(0f, 0.5f);
        iconRect.pivot = new Vector2(0f, 0.5f);
        iconRect.anchoredPosition = new Vector2(14f, 0f);
        iconRect.sizeDelta = new Vector2(38f, 38f);

        coinIcon.sprite = GetCoinSprite();
        coinIcon.preserveAspect = true;
        coinIcon.raycastTarget = false;
        coinIcon.color = Color.white;

        if (coinText == null)
        {
            coinText = CreateHudText(
                panel.transform,
                "Coin Amount",
                30,
                TextAnchor.MiddleLeft,
                Vector2.zero,
                new Vector2(CurrencyPanelWidth - 72f, 44f),
                new Color(1f, 0.93f, 0.62f)
            );
        }
        else
        {
            coinText.transform.SetParent(panel.transform, false);
        }

        StyleCoinText();
    }

    void StyleCoinText()
    {
        if (coinText == null)
            return;

        RectTransform rect = coinText.rectTransform;
        rect.anchorMin = new Vector2(0f, 0.5f);
        rect.anchorMax = new Vector2(0f, 0.5f);
        rect.pivot = new Vector2(0f, 0.5f);
        rect.anchoredPosition = new Vector2(60f, 0f);
        rect.sizeDelta = new Vector2(CurrencyPanelWidth - 74f, 44f);

        Font hudFont =
            timeText != null && timeText.font != null
                ? timeText.font
                : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        if (hudFont != null)
            coinText.font = hudFont;

        coinText.alignment = TextAnchor.MiddleLeft;
        coinText.fontSize = 30;
        coinText.fontStyle = FontStyle.Bold;
        coinText.color = new Color(1f, 0.93f, 0.62f);
        coinText.raycastTarget = false;
        coinText.horizontalOverflow = HorizontalWrapMode.Overflow;
        coinText.verticalOverflow = VerticalWrapMode.Overflow;

        Shadow shadow = coinText.GetComponent<Shadow>();

        if (shadow == null)
            shadow = coinText.gameObject.AddComponent<Shadow>();

        shadow.effectColor = new Color(0f, 0f, 0f, 0.85f);
        shadow.effectDistance = new Vector2(1f, -1f);
    }

    Sprite GetCoinSprite()
    {
        if (coinIcon != null && coinIcon.sprite != null)
            return coinIcon.sprite;

        if (coinSprite != null)
            return coinSprite;

        coinSprite = Resources.Load<Sprite>("coin_sprite");

        if (coinSprite == null)
            coinSprite = CreateFallbackCoinSprite(64);

        return coinSprite;
    }

    static Sprite GetCurrencyPanelSprite()
    {
        if (currencyPanelSprite != null)
            return currencyPanelSprite;

        const int size = 64;
        const int border = 20;
        const float cornerRadius = 20f;

        Texture2D texture =
            new Texture2D(size, size, TextureFormat.RGBA32, false);

        texture.name = "CurrencyPanel";
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;
        texture.hideFlags = HideFlags.HideAndDontSave;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float px = x + 0.5f;
                float py = y + 0.5f;

                float cx = Mathf.Clamp(px, cornerRadius, size - cornerRadius);
                float cy = Mathf.Clamp(py, cornerRadius, size - cornerRadius);

                float distance =
                    Vector2.Distance(
                        new Vector2(px, py),
                        new Vector2(cx, cy)
                    );

                float alpha =
                    Mathf.Clamp01(cornerRadius - distance + 0.5f);

                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        texture.Apply();

        currencyPanelSprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, size, size),
            new Vector2(0.5f, 0.5f),
            100f,
            0,
            SpriteMeshType.FullRect,
            new Vector4(border, border, border, border)
        );

        currencyPanelSprite.name = "CurrencyPanel";
        currencyPanelSprite.hideFlags = HideFlags.HideAndDontSave;

        return currencyPanelSprite;
    }

    static Sprite CreateFallbackCoinSprite(int size)
    {
        if (fallbackCoinSprite != null)
            return fallbackCoinSprite;

        Texture2D texture =
            new Texture2D(size, size, TextureFormat.RGBA32, false);

        texture.name = "CoinFallback";
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;
        texture.hideFlags = HideFlags.HideAndDontSave;

        Color clear = new Color(0f, 0f, 0f, 0f);
        Color rim = new Color(0.56f, 0.37f, 0.08f, 1f);
        Color face = new Color(0.98f, 0.78f, 0.24f, 1f);
        Color shine = new Color(1f, 0.94f, 0.64f, 1f);

        float radius = size * 0.5f;
        Vector2 center = new Vector2(radius, radius);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 pixel = new Vector2(x + 0.5f, y + 0.5f);
                float distance = Vector2.Distance(pixel, center);

                Color color;

                if (distance > radius - 0.5f)
                {
                    color = clear;
                }
                else if (distance > radius * 0.8f)
                {
                    color = rim;
                }
                else if (distance > radius * 0.7f)
                {
                    color = face;
                }
                else
                {
                    color =
                        Color.Lerp(
                            face,
                            shine,
                            1f - distance / (radius * 0.7f)
                        );
                }

                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();

        fallbackCoinSprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, size, size),
            new Vector2(0.5f, 0.5f),
            size
        );

        fallbackCoinSprite.name = "CoinFallback";
        fallbackCoinSprite.hideFlags = HideFlags.HideAndDontSave;

        return fallbackCoinSprite;
    }

    public void UpdateCoinUI(int amount)
    {
        EnsureCurrencyDisplay();

        if (coinText == null)
            return;

        coinText.text = "₱ " + amount.ToString("N0");
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
