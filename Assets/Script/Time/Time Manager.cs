using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance {get; private set; }

    [Header("Internal Clock")]
    [SerializeField]
    GameTimestamp timestamp;
    public float timeScale = 1.0f;

    List<ITimeTracker> listeners = new List<ITimeTracker>();

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
        listeners = new List<ITimeTracker>();
    }
    void Start()
    {
        if (SaveManager.HasSave)
        {
            SaveData saved = SaveManager.Data;

            timestamp = new GameTimestamp(
                saved.year,
                (GameTimestamp.Season)saved.season,
                saved.day,
                saved.hour,
                saved.minute
            );

            timeScale = saved.timeScale > 0f
                ? saved.timeScale
                : 1f;
        }
        else
        {
            timestamp = new GameTimestamp(0, GameTimestamp.Season.Summer, 1, 6, 0);
        }

        StartCoroutine(TimeUpdate());
    }

    IEnumerator TimeUpdate()
    {
        while(true)
        {
            Tick();
            yield return new WaitForSeconds(1/timeScale);
        }
    }

    public void Tick()
    {
        timestamp.UpdateClock();

        NotifyListeners();
    }

    public void AdvanceToNextMorning()
    {
        if (timestamp == null)
            return;

        if (timestamp.hour >= 6)
        {
            AdvanceOneDay();
        }

        timestamp.hour = 6;
        timestamp.minute = 0;

        NotifyListeners();
    }

    void AdvanceOneDay()
    {
        timestamp.day++;

        if (timestamp.day > 30)
        {
            timestamp.day = 1;

            if (timestamp.season == GameTimestamp.Season.Summer)
            {
                timestamp.season = GameTimestamp.Season.Rainy;
            }
            else
            {
                timestamp.season = GameTimestamp.Season.Summer;
                timestamp.year++;
            }
        }
    }

    void NotifyListeners()
    {
        foreach (ITimeTracker listener in
                 new List<ITimeTracker>(listeners))
        {
            if (listener != null)
            {
                listener.ClockUpdate(timestamp);
            }
        }
    }

    public GameTimestamp GetGameTimestamp()
    {
        if (timestamp == null)
            return null;

        return new GameTimestamp(timestamp);
    }

    public void RegisterTracker(ITimeTracker listener)
    {
        listeners.Add(listener);
    }

    public void UnregisterTracker(ITimeTracker listener)
    {
        listeners.Remove(listener);
    }
}
