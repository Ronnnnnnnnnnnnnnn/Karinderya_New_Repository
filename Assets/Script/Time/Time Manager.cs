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

    [Header ("Day and Night cycle")]
    //public Transform sunTransform;

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
        timestamp = new GameTimestamp(0, GameTimestamp.Season.Summer, 1, 6, 0);
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

        foreach(ITimeTracker listener in new List<ITimeTracker>(listeners))
        {
            listener.ClockUpdate(timestamp);
        }
        //UpdateSunMovement();
    }

    /*void UpdateSunMovement()
    {
         int timeInMinute = GameTimestamp.HoursToMinutes(timestamp.hour) + timestamp.minute;

        float sunAngle = .25f * timeInMinute - 90;

        sunTransform.eulerAngles = new Vector3(sunAngle, 0, 0);
    }*/

    public GameTimestamp GetGameTimestamp()
    {
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
