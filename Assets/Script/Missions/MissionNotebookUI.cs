using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MissionNotebookUI : MonoBehaviour
{
    public static MissionNotebookUI Instance { get; private set; }
    public static bool IsOpen { get; private set; }

    public static int SuppressWorldInputUntilFrame
    {
        get;
        private set;
    } = -1;

    public static bool ShouldSuppressWorldInput =>
        Time.frameCount <= SuppressWorldInputUntilFrame;

    class MissionCard
    {
        public GameObject Root;
        public Image Background;
        public Text Title;
        public Text Requester;
        public Text Description;
        public Text Progress;
        public Text Meta;
        public Text Status;

        public void SetMission(
            MissionInstance mission,
            MissionManager manager)
        {
            if (mission == null)
            {
                Root.SetActive(false);
                return;
            }

            Root.SetActive(true);

            MissionDefinition definition =
                mission.Definition;

            Title.text = definition.title;
            Requester.text =
                "from " + definition.requester;
            Description.text =
                definition.description;
            Progress.text =
                manager.GetProgressText(mission);

            Meta.text =
                manager.GetWindowText(definition) +
                "   |   ₱" +
                definition.rewardCoins +
                "   |   +" +
                definition.rewardReputation +
                " trust";

            switch (mission.State)
            {
                case MissionState.Completed:
                    Status.text = "COMPLETE";
                    Status.color = CompleteGreen;
                    Background.color =
                        new Color(0.10f, 0.18f, 0.10f, 0.98f);
                    break;

                case MissionState.Expired:
                    Status.text = "MISSED";
                    Status.color = MissedRed;
                    Background.color =
                        new Color(0.18f, 0.09f, 0.08f, 0.98f);
                    break;

                default:
                    Status.text = "IN PROGRESS";
                    Status.color = Gold;
                    Background.color =
                        new Color(0.17f, 0.13f, 0.09f, 0.98f);
                    break;
            }
        }
    }

    Canvas canvas;
    GameObject blockerRoot;
    GameObject panelRoot;
    Text titleText;
    Text subtitleText;
    Text summaryText;
    Text footerText;
    Button endDayButton;
    Button closeButton;
    GameObject summaryRoot;
    Text summaryBody;
    Button summaryContinueButton;
    bool summaryVisible;

    readonly List<MissionCard> cards =
        new List<MissionCard>();

    float refreshTimer;
    int openedFrame = -1;
    bool gameplayPaused;
    float previousTimeScale = 1f;

    static readonly Color Gold =
        new Color(0.96f, 0.76f, 0.28f, 1f);

    static readonly Color Cream =
        new Color(1f, 0.96f, 0.88f, 1f);

    static readonly Color Muted =
        new Color(0.78f, 0.72f, 0.58f, 1f);

    static readonly Color CompleteGreen =
        new Color(0.45f, 0.85f, 0.45f, 1f);

    static readonly Color MissedRed =
        new Color(0.9f, 0.42f, 0.34f, 1f);

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        Instance = null;
        IsOpen = false;
        SuppressWorldInputUntilFrame = -1;
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (GetComponent<RectTransform>() == null)
        {
            gameObject.AddComponent<RectTransform>();
        }

        BuildUI();
        SetVisible(false);
    }

    void OnDestroy()
    {
        if (gameplayPaused)
            Time.timeScale = previousTimeScale;

        if (Instance == this)
        {
            Instance = null;
            IsOpen = false;
        }
    }

    public static bool OpenNotebook()
    {
        if (Instance == null)
        {
            GameObject uiObject =
                new GameObject(
                    "MissionNotebookUI",
                    typeof(RectTransform)
                );

            uiObject.AddComponent<MissionNotebookUI>();
        }

        if (Instance == null)
            return false;

        Instance.Open();
        return true;
    }

    public static void CloseNotebook()
    {
        if (Instance != null)
            Instance.Close();
    }

    void Open()
    {
        if (panelRoot == null)
            BuildUI();

        if (!PauseMenu.Pause)
        {
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            gameplayPaused = true;
        }

        openedFrame = Time.frameCount;
        SetVisible(true);
        IsOpen = true;
        Refresh();
    }

    void Close()
    {
        SetVisible(false);
        IsOpen = false;

        if (gameplayPaused)
        {
            Time.timeScale = previousTimeScale;
            gameplayPaused = false;
        }

        SuppressWorldInputUntilFrame = Time.frameCount;
    }

    void SetVisible(bool visible)
    {
        if (panelRoot != null)
            panelRoot.SetActive(visible);

        if (blockerRoot != null)
            blockerRoot.SetActive(visible);
    }

    void Update()
    {
        if (!IsOpen)
            return;

        if (Time.frameCount == openedFrame)
            return;

        refreshTimer -= Time.unscaledDeltaTime;

        if (refreshTimer > 0f)
            return;

        refreshTimer = 0.25f;
        Refresh();
    }

    void Refresh()
    {
        MissionManager manager = MissionManager.Instance;

        if (manager == null)
        {
            subtitleText.text = "Requests unavailable.";
            summaryText.text = "";
            footerText.text = "";
            endDayButton.interactable = false;

            if (summaryRoot != null)
                summaryRoot.SetActive(false);

            foreach (MissionCard card in cards)
                card.Root.SetActive(false);

            return;
        }

        if (summaryRoot != null)
        {
            bool showSummary =
                summaryVisible &&
                manager.HasDaySummary;

            summaryRoot.SetActive(showSummary);

            if (showSummary && summaryBody != null)
            {
                summaryBody.text =
                    manager.LastDaySummary +
                    "\n\nNext morning begins.";
            }
        }

        titleText.text = "KARINDERYA REQUESTS";

        subtitleText.text =
            manager.GetDayText() +
            "   |   " +
            manager.GetClockText() +
            "   |   Village trust: " +
            manager.Reputation;

        summaryText.text = manager.GetSummaryText();

        int waiting = manager.ActiveCustomerCount();

        footerText.text =
            waiting > 0
                ? "Finish the " +
                  waiting +
                  " waiting customer(s) before closing."
                : manager.HasGrowingCrops()
                    ? "Harvest or clear growing crops before closing the stall."
                    : manager.AnyActiveMission()
                        ? "Unfinished requests will be missed when you end the day."
                        : "All requests are done. Close the stall for the next day.";

        endDayButton.interactable = manager.IsReady;

        for (int i = 0; i < cards.Count; i++)
        {
            MissionInstance mission =
                i < manager.Missions.Count
                    ? manager.Missions[i]
                    : null;

            cards[i].SetMission(
                mission,
                manager
            );
        }
    }

    void BuildUI()
    {
        if (panelRoot != null)
            return;

        canvas =
            gameObject.GetComponent<Canvas>();

        if (canvas == null)
            canvas = gameObject.AddComponent<Canvas>();

        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;

        CanvasScaler scaler =
            gameObject.GetComponent<CanvasScaler>();

        if (scaler == null)
            scaler = gameObject.AddComponent<CanvasScaler>();

        scaler.uiScaleMode =
            CanvasScaler.ScaleMode.ScaleWithScreenSize;

        scaler.referenceResolution =
            new Vector2(1600f, 900f);

        scaler.matchWidthOrHeight = 0.5f;

        if (gameObject.GetComponent<GraphicRaycaster>() == null)
            gameObject.AddComponent<GraphicRaycaster>();

        Image blocker = CreateImage(
            transform,
            "Blocker",
            new Color(0f, 0f, 0f, 0.56f),
            Vector2.zero,
            new Vector2(4000f, 4000f)
        );

        blocker.raycastTarget = true;
        blockerRoot = blocker.gameObject;

        panelRoot =
            new GameObject(
                "Notebook",
                typeof(RectTransform)
            );

        panelRoot.transform.SetParent(transform, false);

        RectTransform panelRect =
            panelRoot.GetComponent<RectTransform>();

        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(860f, 610f);

        Image panelImage =
            panelRoot.AddComponent<Image>();

        panelImage.color =
            new Color(0.08f, 0.06f, 0.04f, 0.97f);

        Outline panelOutline =
            panelRoot.AddComponent<Outline>();

        panelOutline.effectColor =
            new Color(0.55f, 0.4f, 0.18f, 0.85f);

        panelOutline.effectDistance =
            new Vector2(2f, -2f);

        Image header =
            CreateImage(
                panelRoot.transform,
                "Header",
                new Color(0.16f, 0.11f, 0.06f, 0.98f),
                new Vector2(0f, 255f),
                new Vector2(820f, 70f)
            );

        header.raycastTarget = true;

        titleText = CreateText(
            panelRoot.transform,
            "Title",
            "KARINDERYA REQUESTS",
            30,
            TextAnchor.MiddleCenter,
            Gold,
            new Vector2(0f, 257f),
            new Vector2(760f, 40f),
            FontStyle.Bold
        );

        subtitleText = CreateText(
            panelRoot.transform,
            "Subtitle",
            "",
            19,
            TextAnchor.MiddleCenter,
            Muted,
            new Vector2(0f, 200f),
            new Vector2(800f, 30f)
        );

        cards.Clear();

        cards.Add(
            BuildCard(
                panelRoot.transform,
                75f,
                "Card 1"
            )
        );

        cards.Add(
            BuildCard(
                panelRoot.transform,
                -105f,
                "Card 2"
            )
        );

        summaryText = CreateText(
            panelRoot.transform,
            "Summary",
            "",
            19,
            TextAnchor.MiddleCenter,
            Cream,
            new Vector2(0f, -215f),
            new Vector2(800f, 28f)
        );

        footerText = CreateText(
            panelRoot.transform,
            "Footer Note",
            "",
            16,
            TextAnchor.MiddleCenter,
            Muted,
            new Vector2(0f, -245f),
            new Vector2(800f, 24f)
        );

        endDayButton = CreateButton(
            panelRoot.transform,
            "EndDayButton",
            "Close Stall / End Day",
            new Color(0.42f, 0.27f, 0.10f, 1f),
            new Vector2(-225f, -277f),
            new Vector2(280f, 46f),
            EndDayClicked
        );

        closeButton = CreateButton(
            panelRoot.transform,
            "CloseButton",
            "Close",
            new Color(0.22f, 0.20f, 0.17f, 1f),
            new Vector2(300f, -277f),
            new Vector2(150f, 46f),
            CloseClicked
        );

        BuildSummaryPanel();
    }

    void BuildSummaryPanel()
    {
        summaryRoot =
            new GameObject(
                "DaySummary",
                typeof(RectTransform)
            );

        summaryRoot.transform.SetParent(panelRoot.transform, false);

        RectTransform rect =
            summaryRoot.GetComponent<RectTransform>();

        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(720f, 370f);

        Image background = summaryRoot.AddComponent<Image>();
        background.color =
            new Color(0.11f, 0.08f, 0.05f, 0.99f);
        background.raycastTarget = true;

        Outline outline = summaryRoot.AddComponent<Outline>();
        outline.effectColor =
            new Color(0.72f, 0.53f, 0.2f, 0.9f);
        outline.effectDistance = new Vector2(2f, -2f);

        CreateText(
            summaryRoot.transform,
            "SummaryTitle",
            "DAY COMPLETE",
            34,
            TextAnchor.MiddleCenter,
            Gold,
            new Vector2(0f, 130f),
            new Vector2(640f, 50f),
            FontStyle.Bold
        );

        summaryBody = CreateText(
            summaryRoot.transform,
            "SummaryBody",
            "",
            22,
            TextAnchor.MiddleCenter,
            Cream,
            new Vector2(0f, 20f),
            new Vector2(640f, 180f)
        );

        summaryContinueButton = CreateButton(
            summaryRoot.transform,
            "SummaryContinue",
            "Continue",
            new Color(0.42f, 0.27f, 0.10f, 1f),
            new Vector2(0f, -135f),
            new Vector2(230f, 50f),
            SummaryContinueClicked
        );

        summaryRoot.SetActive(false);
    }

    MissionCard BuildCard(
        Transform parent,
        float y,
        string name)
    {
        GameObject root =
            new GameObject(
                name,
                typeof(RectTransform)
            );

        root.transform.SetParent(parent, false);

        RectTransform rect =
            root.GetComponent<RectTransform>();

        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0f, y);
        rect.sizeDelta = new Vector2(780f, 170f);

        Image background = root.AddComponent<Image>();
        background.color =
            new Color(0.17f, 0.13f, 0.09f, 0.98f);
        background.raycastTarget = true;

        Outline outline = root.AddComponent<Outline>();
        outline.effectColor =
            new Color(0.4f, 0.3f, 0.15f, 0.65f);
        outline.effectDistance = new Vector2(1f, -1f);

        MissionCard card = new MissionCard();
        card.Root = root;
        card.Background = background;

        card.Title = CreateText(
            root.transform,
            "Title",
            "",
            23,
            TextAnchor.MiddleLeft,
            Gold,
            new Vector2(-140f, 55f),
            new Vector2(500f, 32f),
            FontStyle.Bold
        );

        card.Status = CreateText(
            root.transform,
            "Status",
            "",
            17,
            TextAnchor.MiddleRight,
            Gold,
            new Vector2(285f, 55f),
            new Vector2(180f, 30f),
            FontStyle.Bold
        );

        card.Requester = CreateText(
            root.transform,
            "Requester",
            "",
            16,
            TextAnchor.MiddleLeft,
            Muted,
            new Vector2(-170f, 28f),
            new Vector2(440f, 24f)
        );

        card.Description = CreateText(
            root.transform,
            "Description",
            "",
            17,
            TextAnchor.MiddleLeft,
            Cream,
            new Vector2(0f, 0f),
            new Vector2(720f, 38f)
        );

        card.Progress = CreateText(
            root.transform,
            "Progress",
            "",
            17,
            TextAnchor.MiddleLeft,
            Gold,
            new Vector2(0f, -34f),
            new Vector2(720f, 26f),
            FontStyle.Bold
        );

        card.Meta = CreateText(
            root.transform,
            "Meta",
            "",
            15,
            TextAnchor.MiddleLeft,
            Muted,
            new Vector2(0f, -61f),
            new Vector2(720f, 24f)
        );

        return card;
    }

    void EndDayClicked()
    {
        if (MissionManager.Instance == null)
            return;

        bool closed = MissionManager.Instance.EndDay();

        if (closed)
        {
            summaryVisible = true;
            SaveManager.SaveCurrent();
        }

        Refresh();
    }

    void SummaryContinueClicked()
    {
        summaryVisible = false;

        if (summaryRoot != null)
            summaryRoot.SetActive(false);

        Refresh();
    }

    void CloseClicked()
    {
        Close();
    }

    Image CreateImage(
        Transform parent,
        string name,
        Color color,
        Vector2 position,
        Vector2 size)
    {
        GameObject go =
            new GameObject(
                name,
                typeof(RectTransform)
            );

        go.transform.SetParent(parent, false);

        RectTransform rect =
            go.GetComponent<RectTransform>();

        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image image = go.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = false;

        return image;
    }

    Text CreateText(
        Transform parent,
        string name,
        string content,
        int fontSize,
        TextAnchor alignment,
        Color color,
        Vector2 position,
        Vector2 size,
        FontStyle style = FontStyle.Normal)
    {
        GameObject go =
            new GameObject(
                name,
                typeof(RectTransform)
            );

        go.transform.SetParent(parent, false);

        RectTransform rect =
            go.GetComponent<RectTransform>();

        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Text text = go.AddComponent<Text>();
        text.font =
            Resources.GetBuiltinResource<Font>(
                "LegacyRuntime.ttf"
            );

        text.text = content;
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.alignment = alignment;
        text.color = color;
        text.raycastTarget = false;
        text.horizontalOverflow =
            HorizontalWrapMode.Wrap;
        text.verticalOverflow =
            VerticalWrapMode.Overflow;

        return text;
    }

    Button CreateButton(
        Transform parent,
        string name,
        string label,
        Color background,
        Vector2 position,
        Vector2 size,
        UnityAction onClick)
    {
        GameObject go =
            new GameObject(
                name,
                typeof(RectTransform)
            );

        go.transform.SetParent(parent, false);

        RectTransform rect =
            go.GetComponent<RectTransform>();

        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image image = go.AddComponent<Image>();
        image.color = background;

        Button button = go.AddComponent<Button>();
        button.targetGraphic = image;

        ColorBlock colors = button.colors;
        colors.normalColor = background;
        colors.highlightedColor =
            Color.Lerp(background, Color.white, 0.12f);
        colors.pressedColor =
            Color.Lerp(background, Color.black, 0.2f);
        colors.selectedColor = background;
        button.colors = colors;

        button.onClick.AddListener(onClick);

        Text text = CreateText(
            go.transform,
            "Label",
            label,
            18,
            TextAnchor.MiddleCenter,
            Cream,
            Vector2.zero,
            new Vector2(size.x - 12f, size.y - 6f),
            FontStyle.Bold
        );

        text.raycastTarget = false;

        return button;
    }
}
