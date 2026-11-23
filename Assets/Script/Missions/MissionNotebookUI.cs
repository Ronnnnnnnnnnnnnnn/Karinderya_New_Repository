using UnityEngine;
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

    [Header("Scene References")]
    public GameObject blockerRoot;
    public GameObject panelRoot;
    public Text titleText;
    public Text subtitleText;
    public Text summaryText;
    public Text footerText;
    public Button endDayButton;
    public Button closeButton;
    public MissionCardView[] cards;
    public GameObject summaryRoot;
    public Text summaryBody;
    public Button summaryContinueButton;

    float refreshTimer;
    int openedFrame = -1;
    bool gameplayPaused;
    float previousTimeScale = 1f;
    bool summaryVisible;

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
        WireButtons();
        SetVisible(false);
    }

    void Start()
    {
        Refresh();
    }

    void OnDestroy()
    {
        if (gameplayPaused)
            Time.timeScale = previousTimeScale;

        UnwireButtons();

        if (Instance == this)
        {
            Instance = null;
            IsOpen = false;
        }
    }

    void WireButtons()
    {
        if (endDayButton != null)
            endDayButton.onClick.AddListener(EndDayClicked);

        if (closeButton != null)
            closeButton.onClick.AddListener(CloseClicked);

        if (summaryContinueButton != null)
        {
            summaryContinueButton.onClick.AddListener(
                SummaryContinueClicked
            );
        }
    }

    void UnwireButtons()
    {
        if (endDayButton != null)
            endDayButton.onClick.RemoveListener(EndDayClicked);

        if (closeButton != null)
            closeButton.onClick.RemoveListener(CloseClicked);

        if (summaryContinueButton != null)
        {
            summaryContinueButton.onClick.RemoveListener(
                SummaryContinueClicked
            );
        }
    }

    public static bool OpenNotebook()
    {
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
            return;

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
            if (subtitleText != null)
                subtitleText.text = "Requests unavailable.";

            if (summaryText != null)
                summaryText.text = "";

            if (footerText != null)
                footerText.text = "";

            if (endDayButton != null)
                endDayButton.interactable = false;

            if (summaryRoot != null)
                summaryRoot.SetActive(false);

            HideCards();
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

        if (titleText != null)
            titleText.text = "KARINDERYA REQUESTS";

        if (subtitleText != null)
        {
            subtitleText.text =
                manager.GetDayText() +
                "   |   " +
                manager.GetClockText() +
                "   |   Village trust: " +
                manager.Reputation;
        }

        if (summaryText != null)
            summaryText.text = manager.GetSummaryText();

        int waiting = manager.ActiveCustomerCount();

        if (footerText != null)
        {
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
        }

        if (endDayButton != null)
            endDayButton.interactable = manager.IsReady;

        if (cards == null)
            return;

        for (int i = 0; i < cards.Length; i++)
        {
            if (cards[i] == null)
                continue;

            MissionInstance mission =
                i < manager.Missions.Count
                    ? manager.Missions[i]
                    : null;

            cards[i].SetMission(mission, manager);
        }
    }

    void HideCards()
    {
        if (cards == null)
            return;

        foreach (MissionCardView card in cards)
        {
            if (card != null)
                card.gameObject.SetActive(false);
        }
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
}
