using System;
using UnityEngine;

public enum MissionObjectiveType
{
    ServeCustomers,
    ServeDish,
    EarnFromCustomers,
    NoAngryCustomers
}

public enum MissionState
{
    Active,
    Completed,
    Expired
}

[Serializable]
public class MissionDefinition
{
    public string id;
    public string title;
    public string requester;

    [TextArea]
    public string description;

    public MissionObjectiveType objectiveType;

    [Tooltip("Only used by ServeDish missions.")]
    public ItemData targetServing;

    [Tooltip("Display name used in progress text.")]
    public string targetName;

    public int targetAmount;
    public int startHour;
    public int endHour;
    public int rewardCoins;
    public int rewardReputation;
}

public class MissionInstance
{
    public MissionDefinition Definition;
    public int Progress;
    public MissionState State = MissionState.Active;

    public bool IsActive =>
        State == MissionState.Active;

    public bool IsCompleted =>
        State == MissionState.Completed;

    public bool IsExpired =>
        State == MissionState.Expired;
}
