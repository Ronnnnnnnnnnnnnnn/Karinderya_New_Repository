using UnityEngine;
using UnityEngine.UI;

public class HudStatusUI : MonoBehaviour
{
    public GameObject panel;
    public Text phaseText;
    public Text missionText;

    float refreshTimer;
    TutorialManager tutorial;

    void Start()
    {
        tutorial = FindFirstObjectByType<TutorialManager>();
        Refresh();
    }

    void Update()
    {
        if (panel == null)
            return;

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
