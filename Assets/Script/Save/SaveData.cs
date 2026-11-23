using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public int version = 1;

    public bool tutorialCompleted;

    public int year;
    public int season;
    public int day;
    public int hour;
    public int minute;
    public float timeScale = 1f;

    public int coins;
    public int dishesServed;
    public int reputation;

    public int currentMissionDay;
    public bool missionsReady;
    public int servedToday;
    public int earnedToday;
    public int completedToday;
    public int expiredToday;
    public int missionCoinsToday;
    public int trustGainedToday;
    public bool angryToday;
    public string lastDaySummary = "";

    public List<string> unlockedRecipes =
        new List<string>();

    public List<int> claimedTrustRewards =
        new List<int>();

    public List<SlotSave> toolSlots =
        new List<SlotSave>();

    public List<SlotSave> itemSlots =
        new List<SlotSave>();

    public SlotSave equippedTool =
        new SlotSave();

    public SlotSave equippedItem =
        new SlotSave();

    public List<LandSlotSave> land =
        new List<LandSlotSave>();

    public List<CropSlotSave> crops =
        new List<CropSlotSave>();

    public List<BuffetSlotSave> buffets =
        new List<BuffetSlotSave>();

    public List<MissionSlotSave> missions =
        new List<MissionSlotSave>();
}

[Serializable]
public class SlotSave
{
    public string itemName;
    public int quantity;
}

[Serializable]
public class LandSlotSave
{
    public int status;
    public int year;
    public int season;
    public int day;
    public int hour;
    public int minute;
}

[Serializable]
public class CropSlotSave
{
    public int landID;
    public string seedName;
    public int cropState;
    public float growth;
    public float health;
}

[Serializable]
public class BuffetSlotSave
{
    public string recipeName;
    public string storedDishName;
    public int servings;
}

[Serializable]
public class MissionSlotSave
{
    public string id;
    public string title;
    public string requester;
    public string description;
    public int objectiveType;
    public string targetServingName;
    public string targetName;
    public int targetAmount;
    public int startHour;
    public int endHour;
    public int rewardCoins;
    public int rewardReputation;
    public int progress;
    public int state;
}
