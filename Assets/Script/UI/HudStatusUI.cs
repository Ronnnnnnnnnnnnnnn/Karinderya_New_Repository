using UnityEngine;
using UnityEngine.UI;

public class HudStatusUI : MonoBehaviour
{
    GameObject panel;
    Text phaseText;
    Text missionText;
    float refreshTimer;
    TutorialManager tutorial;

    void Start()
    {
        tutorial = FindFirstObjectByType<TutorialManager>();
        Build();
    }

    void Build()
    {
        Canvas canvas = UIRoot.FindGameCanvas();

        if (canvas == null)
            return;

        panel = new GameObject("RushStatus", typeof(RectTransform));
        panel.transform.SetParent(canvas.transform, false);

        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, -116f);
        rect.sizeDelta = new Vector2(560f, 74f);

        Image background = panel.AddComponent<Image>();
        background.color = new Color(0.08f, 0.1f, 0.07f, 0.84f);
        background.raycastTarget = false;

        phaseText = CreateText(
            panel.transform,
            "Phase",
            22,
            new Vector2(0f, -8f),
            new Vector2(520f, 28f),
            new Color(0.96f, 0.76f, 0.28f, 1f)
        );

        missionText = CreateText(
            panel.transform,
            "Missions",
            16,
            new Vector2(0f, -38f),
            new Vector2(520f, 24f),
            new Color(0.94f, 0.88f, 0.72f, 1f)
        );

        Refresh();
    }

    Text CreateText(
        Transform parent,
        string name,
        int size,
        Vector2 position,
        Vector2 dimensions,
        Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);

        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = position;
        rect.sizeDelta = dimensions;

        Text text = go.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = size;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = color;
        text.raycastTarget = false;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;

        Shadow shadow = go.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.85f);
        shadow.effectDistance = new Vector2(1f, -1f);

        return text;
    }

    void Update()
    {
        if (panel == null)
        {
            Build();

            if (panel == null)
                return;
        }

        if (tutorial == null)
            tutorial = FindFirstObjectByType<TutorialManager>();

        bool hide =
            PauseMenu.Pause ||
            PauseMenu.InMainMenu ||
            MissionNotebookUI.IsOpen ||
            (tutorial != null && !tutorial.IsFinished) ||
            (UIManager.Instance != null &&
             UIManager.Instance.IsMenuOpen) ||
            (CookingUIManager.Instance != null &&
             CookingUIManager.Instance.IsOpen);

        if (panel.activeSelf == hide)
            panel.SetActive(!hide);

        if (hide)
        {
            refreshTimer = 0f;
            return;
        }

        refreshTimer -= Time.unscaledDeltaTime;

        if (refreshTimer > 0f)
            return;

        refreshTimer = 0.25f;
        Refresh();
    }

    static string FormatDuration(int minutes)
    {
        minutes = Mathf.Max(0, minutes);

        int hours = minutes / 60;
        int remainder = minutes % 60;

        if (hours <= 0)
            return remainder + "m";

        if (remainder <= 0)
            return hours + "h";

        return hours + "h " + remainder + "m";
    }

    void Refresh()
    {
        if (phaseText == null || missionText == null)
            return;

        TimeManager time = TimeManager.Instance;

        if (time == null)
        {
            phaseText.text = "";
            missionText.text = "";
            return;
        }

        GameTimestamp timestamp = time.GetGameTimestamp();

        if (timestamp == null)
        {
            phaseText.text = "";
            missionText.text = "";
            return;
        }

        int minuteOfDay = timestamp.hour * 60 + timestamp.minute;
        string phase;
        bool rush = false;

        if (timestamp.hour >= 11 && timestamp.hour < 14)
        {
            phase =
                "LUNCH RUSH · " +
                FormatDuration(14 * 60 - minuteOfDay) +
                " left";
            rush = true;
        }
        else if (timestamp.hour >= 17 && timestamp.hour < 21)
        {
            phase =
                "DINNER RUSH · " +
                FormatDuration(21 * 60 - minuteOfDay) +
                " left";
            rush = true;
        }
        else if (timestamp.hour >= 6 && timestamp.hour < 11)
        {
            phase =
                "Lunch rush in " +
                FormatDuration(11 * 60 - minuteOfDay);
        }
        else if (timestamp.hour >= 14 && timestamp.hour < 17)
        {
            phase =
                "Dinner rush in " +
                FormatDuration(17 * 60 - minuteOfDay);
        }
        else if (timestamp.hour >= 21 && timestamp.hour < 22)
        {
            phase = "Closing time";
        }
        else
        {
            phase = "Late night · restock or sleep";
        }

        phaseText.text = phase;
        phaseText.color = rush
            ? new Color(1f, 0.55f, 0.25f, 1f)
            : new Color(0.96f, 0.76f, 0.28f, 1f);

        MissionManager missions = MissionManager.Instance;

        if (missions == null || !missions.IsReady)
        {
            missionText.text = "Requests unlock after the tutorial.";
            return;
        }

        int active = 0;

        foreach (MissionInstance mission in missions.Missions)
        {
            if (mission.IsActive)
                active++;
        }

        missionText.text =
            active +
            " requests active   |   Trust " +
            missions.Reputation;
    }
}
