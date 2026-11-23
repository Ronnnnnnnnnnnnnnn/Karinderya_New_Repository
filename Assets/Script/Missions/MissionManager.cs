using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionManager : MonoBehaviour, ITimeTracker
{
    public static MissionManager Instance { get; private set; }

    [Header("Daily Requests")]
    [Min(1)]
    public int dailyMissionCount = 2;

    public int startingReputation = 10;
    public int maxReputation = 100;

    [Header("Trust Changes")]
    public int angryReputationPenalty = 1;
    public int missedReputationPenalty = 1;

    [Header("Daily Costs")]
    public int dailyStallFee = 25;

    [Header("Regulars")]
    public float regularChance = 0.25f;
    public float regularTipMultiplier = 1.25f;
    public float regularPatienceBonus = 15f;

    readonly List<MissionInstance> missions =
        new List<MissionInstance>();

    readonly List<int> claimedTrustRewards =
        new List<int>();

    TimeManager timeManager;
    TutorialManager tutorial;
    bool timeRegistered;
    bool missionsReady;

    int currentMissionDay = -1;
    int lastFinalizedDay = int.MinValue;
    int servedToday;
    int earnedToday;
    bool angryToday;
    int completedToday;
    int expiredToday;
    int missionCoinsToday;
    int trustGainedToday;

    public IReadOnlyList<MissionInstance> Missions => missions;
    public int Reputation { get; private set; }
    public int ServedToday => servedToday;
    public int EarnedToday => earnedToday;
    public bool AngryToday => angryToday;
    public int CompletedToday => completedToday;
    public int ExpiredToday => expiredToday;
    public bool IsReady => missionsReady;
    public bool RegularsUnlocked { get; private set; }
    public string LastDaySummary { get; private set; } = "";
    public int LastDayCoins { get; private set; }
    public int LastDayTrust { get; private set; }
    public int LastDayCompleted { get; private set; }
    public bool HasDaySummary =>
        !string.IsNullOrEmpty(LastDaySummary);

    static readonly string[] VillagerNames =
    {
        "Aling Nena",
        "Mang Tonyo",
        "Aling Rosa",
        "Mang Ben"
    };

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        Instance = null;
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Reputation = startingReputation;
        dailyMissionCount = Mathf.Clamp(
            dailyMissionCount,
            1,
            2
        );

        CustomerOrder.CustomerFinished +=
            OnCustomerFinished;
    }

    void Start()
    {
        tutorial =
            FindFirstObjectByType<TutorialManager>();

        TryRegisterTime();

        StartCoroutine(BeginWhenReady());
    }

    void OnDestroy()
    {
        CustomerOrder.CustomerFinished -=
            OnCustomerFinished;

        if (timeManager != null)
        {
            timeManager.UnregisterTracker(this);
        }

        if (Instance == this)
            Instance = null;
    }

    IEnumerator BeginWhenReady()
    {
        float waitStarted = Time.unscaledTime;

        while (true)
        {
            if (TimeManager.Instance != null)
            {
                TryRegisterTime();

                if (tutorial == null)
                {
                    tutorial =
                        FindFirstObjectByType<TutorialManager>();
                }

                bool tutorialReady =
                    tutorial != null &&
                    tutorial.IsFinished;

                bool tutorialMissing =
                    tutorial == null &&
                    Time.unscaledTime - waitStarted > 2f;

                if (tutorialReady || tutorialMissing)
                {
                    GameTimestamp timestamp =
                        TimeManager.Instance
                        .GetGameTimestamp();

                    if (timestamp != null)
                    {
                        if (SaveManager.HasSave &&
                            SaveManager.Data != null &&
                            SaveManager.Data.missionsReady)
                        {
                            LoadFromSave(SaveManager.Data);
                        }
                        else
                        {
                            StartDay(timestamp);
                        }

                        yield break;
                    }
                }
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    void TryRegisterTime()
    {
        if (timeRegistered ||
            TimeManager.Instance == null)
        {
            return;
        }

        timeManager = TimeManager.Instance;
        timeManager.RegisterTracker(this);
        timeRegistered = true;
    }

    public void ClockUpdate(GameTimestamp timestamp)
    {
        if (!missionsReady)
            return;

        if (timestamp.hour >= 6 &&
            timestamp.day != currentMissionDay)
        {
            FinalizeDay();
            StartDay(timestamp);
            return;
        }

        CheckMissionExpirations(timestamp);
    }

    void StartDay(GameTimestamp timestamp)
    {
        if (timestamp == null)
            return;

        if (missionsReady &&
            currentMissionDay == timestamp.day)
        {
            return;
        }

        missions.Clear();

        servedToday = 0;
        earnedToday = 0;
        angryToday = false;
        completedToday = 0;
        expiredToday = 0;
        missionCoinsToday = 0;
        trustGainedToday = 0;

        BuildDailyMissions(timestamp);

        currentMissionDay = timestamp.day;
        missionsReady = true;

        CheckTrustRewards();

        Notify(
            "Day " +
            timestamp.day +
            ": new requests are on the front board."
        );

        Debug.Log(
            "[MISSIONS] Day " +
            timestamp.day +
            " started with " +
            missions.Count +
            " requests."
        );
    }

    public bool EndDay()
    {
        if (!missionsReady ||
            TimeManager.Instance == null)
        {
            Notify("Requests are not ready yet.");
            return false;
        }

        CustomerSpawner spawner =
            FindFirstObjectByType<CustomerSpawner>();

        if (spawner != null &&
            spawner.GetCurrentCustomers() > 0)
        {
            Notify(
                "Finish serving the customers first."
            );

            return false;
        }

        if (HasGrowingCrops())
        {
            Notify(
                "Harvest or clear the growing crops before closing the stall."
            );

            return false;
        }

        FinalizeDay();
        TimeManager.Instance.AdvanceToNextMorning();

        return true;
    }

    void FinalizeDay()
    {
        if (!missionsReady)
            return;

        if (lastFinalizedDay == currentMissionDay)
            return;

        lastFinalizedDay = currentMissionDay;

        foreach (MissionInstance mission in missions)
        {
            if (!mission.IsActive)
                continue;

            if (mission.Definition.objectiveType ==
                MissionObjectiveType.NoAngryCustomers)
            {
                if (servedToday > 0 && !angryToday)
                {
                    CompleteMission(mission);
                }
                else
                {
                    ExpireMission(mission, false);
                }
            }
            else
            {
                ExpireMission(mission, false);
            }
        }

        int feeCharged = 0;

        if (CurrencyManager.Instance != null &&
            dailyStallFee > 0)
        {
            feeCharged = Mathf.Min(
                dailyStallFee,
                CurrencyManager.Instance.coins
            );

            if (feeCharged > 0)
                CurrencyManager.Instance.AddCoins(-feeCharged);
        }

        int netCoins =
            earnedToday +
            missionCoinsToday -
            feeCharged;

        LastDayCoins = netCoins;
        LastDayTrust = trustGainedToday;
        LastDayCompleted = completedToday;

        string summary =
            "Day " +
            currentMissionDay +
            " complete - " +
            completedToday +
            "/" +
            missions.Count +
            " requests · ₱" +
            netCoins +
            " · +" +
            trustGainedToday +
            " trust";

        if (feeCharged > 0)
            summary += " · stall fee ₱" + feeCharged;

        LastDaySummary = summary;

        if (completedToday == missions.Count)
            summary += " · Perfect day!";

        Notify(summary);

        Debug.Log("[MISSIONS] " + summary);
    }

    void OnCustomerFinished(
        CustomerOrder order,
        bool served)
    {
        if (!missionsReady ||
            TimeManager.Instance == null ||
            order == null)
        {
            return;
        }

        if (!served)
        {
            if (!angryToday)
            {
                angryToday = true;

                Reputation = Mathf.Max(
                    0,
                    Reputation -
                    angryReputationPenalty
                );

                Notify(
                    "A customer left upset. -" +
                    angryReputationPenalty +
                    " trust."
                );
            }

            foreach (MissionInstance mission in missions)
            {
                if (mission.IsActive &&
                    mission.Definition.objectiveType ==
                    MissionObjectiveType.NoAngryCustomers)
                {
                    ExpireMission(mission, true);
                }
            }

            return;
        }

        servedToday++;
        earnedToday += Mathf.Max(0, order.rewardCoins);

        GameTimestamp timestamp =
            TimeManager.Instance.GetGameTimestamp();

        foreach (MissionInstance mission in missions)
        {
            if (!mission.IsActive)
                continue;

            MissionDefinition definition =
                mission.Definition;

            if (!IsInsideMissionWindow(
                    timestamp.hour,
                    definition))
            {
                continue;
            }

            bool changed = false;

            switch (definition.objectiveType)
            {
                case MissionObjectiveType.ServeCustomers:
                    mission.Progress++;
                    changed = true;
                    break;

                case MissionObjectiveType.ServeDish:
                    if (MatchesDish(
                            order.requestedServing,
                            definition.targetServing))
                    {
                        mission.Progress++;
                        changed = true;
                    }
                    break;

                case MissionObjectiveType.EarnFromCustomers:
                    mission.Progress +=
                        Mathf.Max(0, order.rewardCoins);
                    changed = true;
                    break;

                case MissionObjectiveType.NoAngryCustomers:
                    break;
            }

            if (changed &&
                mission.Progress >=
                definition.targetAmount)
            {
                CompleteMission(mission);
            }
        }
    }

    static bool MatchesDish(
        ItemData serving,
        ItemData target)
    {
        if (serving == null || target == null)
            return false;

        if (serving == target)
            return true;

        if (target.servingVersion == serving)
            return true;

        if (serving.servingVersion == target)
            return true;

        return false;
    }

    void CheckMissionExpirations(
        GameTimestamp timestamp)
    {
        foreach (MissionInstance mission in missions)
        {
            if (!mission.IsActive)
                continue;

            MissionDefinition definition =
                mission.Definition;

            if (definition.objectiveType ==
                MissionObjectiveType.NoAngryCustomers)
            {
                continue;
            }

            if (HasWindowEnded(
                    timestamp.hour,
                    definition))
            {
                ExpireMission(mission, true);
            }
        }
    }

    static bool HasWindowEnded(
        int hour,
        MissionDefinition definition)
    {
        int start = definition.startHour;
        int end = definition.endHour;

        if (end >= 24 || start == end)
            return false;

        if (start < end)
            return hour >= end;

        return hour >= end && hour < start;
    }

    static bool IsInsideMissionWindow(
        int hour,
        MissionDefinition definition)
    {
        int start = definition.startHour;
        int end = definition.endHour;

        if (start == end)
            return true;

        if (start < end)
        {
            return hour >= start && hour < end;
        }

        return hour >= start || hour < end;
    }

    void CompleteMission(MissionInstance mission)
    {
        if (mission == null || !mission.IsActive)
            return;

        mission.State = MissionState.Completed;
        completedToday++;

        MissionDefinition definition =
            mission.Definition;

        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.AddCoins(
                definition.rewardCoins
            );
        }

        Reputation = Mathf.Clamp(
            Reputation + definition.rewardReputation,
            0,
            maxReputation
        );

        missionCoinsToday += definition.rewardCoins;
        trustGainedToday += definition.rewardReputation;

        CheckTrustRewards();

        Notify(
            "Request complete: " +
            definition.title +
            "  +₱" +
            definition.rewardCoins +
            "  +" +
            definition.rewardReputation +
            " trust."
        );

        Debug.Log(
            "[MISSIONS] Completed: " +
            definition.title
        );
    }

    void ExpireMission(
        MissionInstance mission,
        bool notify)
    {
        if (mission == null || !mission.IsActive)
            return;

        mission.State = MissionState.Expired;
        expiredToday++;

        Reputation = Mathf.Max(
            0,
            Reputation - missedReputationPenalty
        );

        if (notify)
        {
            Notify(
                "Request missed: " +
                mission.Definition.title +
                "  -" +
                missedReputationPenalty +
                " trust."
            );
        }

        Debug.Log(
            "[MISSIONS] Expired: " +
            mission.Definition.title
        );
    }

    void CheckTrustRewards()
    {
        int[] thresholds = { 20, 40, 60, 80 };

        foreach (int threshold in thresholds)
        {
            if (Reputation < threshold ||
                claimedTrustRewards.Contains(threshold))
            {
                continue;
            }

            claimedTrustRewards.Add(threshold);

            switch (threshold)
            {
                case 20:
                    GrantRecipeReward("Pakbet", 200);
                    break;

                case 40:
                    RegularsUnlocked = true;
                    Notify(
                        "Trust 40: regular customers now visit " +
                        "the karinderya."
                    );
                    break;

                case 60:
                    GrantRecipeReward("Dinengdeng", 300);
                    break;

                case 80:
                    GrantRecipeReward("Sinigang", 400);
                    break;
            }
        }
    }

    void GrantRecipeReward(
        string recipeName,
        int fallbackCoins)
    {
        RecipeUnlockManager unlockManager =
            RecipeUnlockManager.Instance;

        RecipeData recipe =
            unlockManager != null
                ? unlockManager.FindRecipeByName(recipeName)
                : null;

        if (recipe != null &&
            unlockManager.GrantRecipe(recipe))
        {
            Notify(
                "Trust reward: " +
                recipeName +
                " recipe unlocked!"
            );

            return;
        }

        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.AddCoins(fallbackCoins);

        Notify(
            "Trust reward: +₱" +
            fallbackCoins +
            "."
        );
    }

    MissionDefinition BuildCateringMission()
    {
        return new MissionDefinition
        {
            id = "barangay_catering",
            title = "Barangay Catering",
            requester = "Barangay Hall",
            description =
                "Serve 5 customers for the barangay meeting today.",
            objectiveType =
                MissionObjectiveType.ServeCustomers,
            targetAmount = 5,
            startHour = 6,
            endHour = 21,
            rewardCoins = 500,
            rewardReputation = 15
        };
    }

    void BuildDailyMissions(GameTimestamp timestamp)
    {
        int cycle =
            ((timestamp.day - 1) % 3 + 3) % 3;

        switch (cycle)
        {
            case 0:
                AddRushMission(timestamp.hour);
                AddDishMissionOrFallback(timestamp);
                break;

            case 1:
                if (timestamp.hour < 21)
                {
                    AddMission(
                        BuildDinnerMission()
                    );
                }
                else
                {
                    AddDishMissionOrFallback(timestamp);
                }

                AddMission(
                    BuildEarnMission()
                );
                break;

            default:
                if (Reputation >= 60)
                {
                    AddMission(
                        BuildCateringMission()
                    );
                }
                else
                {
                    AddMission(
                        BuildNoAngryMission()
                    );
                }

                AddDishMissionOrFallback(timestamp);
                break;
        }

        if (missions.Count < dailyMissionCount)
        {
            AddMission(BuildEarnMission());
        }

        if (missions.Count < dailyMissionCount)
        {
            AddMission(BuildNoAngryMission());
        }
    }

    void AddRushMission(int hour)
    {
        if (hour < 14)
        {
            AddMission(BuildLunchMission());
        }
        else if (hour < 21)
        {
            AddMission(BuildDinnerMission());
        }
        else
        {
            AddMission(BuildEarnMission());
        }
    }

    void AddDishMissionOrFallback(
        GameTimestamp timestamp)
    {
        MissionDefinition dishMission =
            BuildDishMission(timestamp);

        if (dishMission != null)
        {
            AddMission(dishMission);
        }
        else
        {
            AddMission(
                BuildEarnMission()
            );
        }
    }

    void AddMission(MissionDefinition definition)
    {
        if (definition == null ||
            missions.Count >= dailyMissionCount)
        {
            return;
        }

        foreach (MissionInstance existing in missions)
        {
            if (existing.Definition != null &&
                existing.Definition.id == definition.id)
            {
                return;
            }
        }

        missions.Add(
            new MissionInstance
            {
                Definition = definition,
                Progress = 0,
                State = MissionState.Active
            }
        );
    }

    MissionDefinition BuildLunchMission()
    {
        return new MissionDefinition
        {
            id = "lunch_crowd",
            title = "Lunch Crowd",
            requester = "Barangay Notes",
            description =
                "Serve 2 customers during the lunch rush.",
            objectiveType =
                MissionObjectiveType.ServeCustomers,
            targetAmount = 2,
            startHour = 11,
            endHour = 14,
            rewardCoins = 120,
            rewardReputation = 5
        };
    }

    MissionDefinition BuildDinnerMission()
    {
        return new MissionDefinition
        {
            id = "dinner_full_house",
            title = "Dinner Full House",
            requester = "Barangay Notes",
            description =
                "Serve 3 customers during the dinner rush.",
            objectiveType =
                MissionObjectiveType.ServeCustomers,
            targetAmount = 3,
            startHour = 17,
            endHour = 21,
            rewardCoins = 200,
            rewardReputation = 8
        };
    }

    MissionDefinition BuildEarnMission()
    {
        return new MissionDefinition
        {
            id = "market_money",
            title = "Market Money",
            requester = "Tindera Notes",
            description =
                "Earn ₱250 from customers today.",
            objectiveType =
                MissionObjectiveType.EarnFromCustomers,
            targetAmount = 250,
            targetName = "customer earnings",
            startHour = 6,
            endHour = 24,
            rewardCoins = 100,
            rewardReputation = 5
        };
    }

    MissionDefinition BuildNoAngryMission()
    {
        return new MissionDefinition
        {
            id = "clean_shift",
            title = "Clean Shift",
            requester = "Village Council",
            description =
                "Serve at least 1 customer and finish " +
                "the day with no angry customers.",
            objectiveType =
                MissionObjectiveType.NoAngryCustomers,
            targetAmount = 1,
            targetName = "clean service",
            startHour = 6,
            endHour = 24,
            rewardCoins = 120,
            rewardReputation = 8
        };
    }

    MissionDefinition BuildDishMission(
        GameTimestamp timestamp)
    {
        RecipeBookCatalog catalog =
            RecipeBookCatalog.Find();

        if (catalog == null ||
            catalog.recipes == null ||
            RecipeUnlockManager.Instance == null)
        {
            return null;
        }

        BuffetContainer[] buffets =
            FindObjectsByType<BuffetContainer>(
                FindObjectsSortMode.None
            );

        List<RecipeData> candidates =
            new List<RecipeData>();

        foreach (RecipeData recipe in catalog.recipes)
        {
            if (recipe == null ||
                recipe.resultDish == null ||
                recipe.resultDish.servingVersion == null ||
                !RecipeUnlockManager.Instance
                    .IsUnlocked(recipe) ||
                !HasBuffetForRecipe(recipe, buffets))
            {
                continue;
            }

            candidates.Add(recipe);
        }

        if (candidates.Count == 0)
            return null;

        int index =
            ((timestamp.day + 2) % candidates.Count +
             candidates.Count) %
            candidates.Count;

        RecipeData chosen = candidates[index];

        string requester =
            VillagerNames[
                (timestamp.day - 1) %
                VillagerNames.Length
            ];

        return new MissionDefinition
        {
            id = "dish_" + chosen.recipeName,
            title = requester + "'s Request",
            requester = requester,
            description =
                "Serve 1 serving of " +
                chosen.recipeName +
                " before the day ends.",
            objectiveType =
                MissionObjectiveType.ServeDish,
            targetServing =
                chosen.resultDish.servingVersion != null
                    ? chosen.resultDish.servingVersion
                    : chosen.resultDish,
            targetName = chosen.recipeName,
            targetAmount = 1,
            startHour = 6,
            endHour = 24,
            rewardCoins = 150,
            rewardReputation = 6
        };
    }

    static bool HasBuffetForRecipe(
        RecipeData recipe,
        BuffetContainer[] buffets)
    {
        if (buffets == null)
            return false;

        foreach (BuffetContainer buffet in buffets)
        {
            if (buffet != null &&
                buffet.buffetRecipe == recipe)
            {
                return true;
            }
        }

        return false;
    }

    public string GetProgressText(
        MissionInstance mission)
    {
        if (mission == null)
            return "";

        MissionDefinition definition =
            mission.Definition;

        switch (definition.objectiveType)
        {
            case MissionObjectiveType.ServeCustomers:
                return
                    "Customers served: " +
                    Mathf.Min(
                        mission.Progress,
                        definition.targetAmount
                    ) +
                    "/" +
                    definition.targetAmount;

            case MissionObjectiveType.ServeDish:
                return
                    "Served: " +
                    Mathf.Min(
                        mission.Progress,
                        definition.targetAmount
                    ) +
                    "/" +
                    definition.targetAmount +
                    " " +
                    definition.targetName;

            case MissionObjectiveType.EarnFromCustomers:
                return
                    "Earned: ₱" +
                    Mathf.Min(
                        mission.Progress,
                        definition.targetAmount
                    ) +
                    "/₱" +
                    definition.targetAmount;

            case MissionObjectiveType.NoAngryCustomers:
                if (angryToday)
                    return "A customer left upset.";
                if (servedToday <= 0)
                    return "Serve at least 1 customer.";
                return "No angry customers so far.";

            default:
                return "";
        }
    }

    public string GetWindowText(
        MissionDefinition definition)
    {
        if (definition == null ||
            definition.endHour >= 24 ||
            definition.startHour ==
            definition.endHour)
        {
            return "All day";
        }

        return
            FormatHour(definition.startHour) +
            " - " +
            FormatHour(definition.endHour);
    }

    static string FormatHour(int hour)
    {
        hour = ((hour % 24) + 24) % 24;

        string suffix =
            hour >= 12 ? "PM" : "AM";

        int display =
            hour % 12;

        if (display == 0)
            display = 12;

        return display + " " + suffix;
    }

    public string GetDayText()
    {
        if (TimeManager.Instance == null)
            return "Day ?";

        GameTimestamp timestamp =
            TimeManager.Instance.GetGameTimestamp();

        if (timestamp == null)
            return "Day ?";

        return
            timestamp.season +
            " " +
            timestamp.day +
            " (" +
            timestamp.GetDayOfTheWeek() +
            ")";
    }

    public string GetClockText()
    {
        if (TimeManager.Instance == null)
            return "";

        GameTimestamp timestamp =
            TimeManager.Instance.GetGameTimestamp();

        if (timestamp == null)
            return "";

        int hours = timestamp.hour;
        string prefix = hours >= 12 ? "PM" : "AM";

        hours %= 12;

        if (hours == 0)
            hours = 12;

        return
            hours +
            ":" +
            timestamp.minute.ToString("00") +
            " " +
            prefix;
    }

    public void WriteSave(SaveData data)
    {
        if (data == null)
            return;

        data.reputation = Reputation;
        data.currentMissionDay = currentMissionDay;
        data.missionsReady = missionsReady;
        data.servedToday = servedToday;
        data.earnedToday = earnedToday;
        data.completedToday = completedToday;
        data.expiredToday = expiredToday;
        data.missionCoinsToday = missionCoinsToday;
        data.trustGainedToday = trustGainedToday;
        data.angryToday = angryToday;
        data.lastDaySummary = LastDaySummary;
        data.claimedTrustRewards =
            new List<int>(claimedTrustRewards);
        data.missions = new List<MissionSlotSave>();

        foreach (MissionInstance mission in missions)
        {
            if (mission == null || mission.Definition == null)
                continue;

            MissionDefinition definition = mission.Definition;

            data.missions.Add(
                new MissionSlotSave
                {
                    id = definition.id,
                    title = definition.title,
                    requester = definition.requester,
                    description = definition.description,
                    objectiveType = (int)definition.objectiveType,
                    targetServingName = definition.targetServing != null
                        ? definition.targetServing.name
                        : "",
                    targetName = definition.targetName,
                    targetAmount = definition.targetAmount,
                    startHour = definition.startHour,
                    endHour = definition.endHour,
                    rewardCoins = definition.rewardCoins,
                    rewardReputation = definition.rewardReputation,
                    progress = mission.Progress,
                    state = (int)mission.State
                }
            );
        }
    }

    public void LoadFromSave(SaveData data)
    {
        if (data == null)
            return;

        Reputation = data.reputation;
        currentMissionDay = data.currentMissionDay;
        missionsReady = data.missionsReady;
        servedToday = data.servedToday;
        earnedToday = data.earnedToday;
        completedToday = data.completedToday;
        expiredToday = data.expiredToday;
        missionCoinsToday = data.missionCoinsToday;
        trustGainedToday = data.trustGainedToday;
        angryToday = data.angryToday;
        LastDaySummary = data.lastDaySummary ?? "";

        claimedTrustRewards.Clear();

        if (data.claimedTrustRewards != null)
        {
            foreach (int threshold in data.claimedTrustRewards)
                claimedTrustRewards.Add(threshold);
        }

        RegularsUnlocked = claimedTrustRewards.Contains(40);

        missions.Clear();

        if (data.missions != null)
        {
            foreach (MissionSlotSave saved in data.missions)
            {
                if (saved == null)
                    continue;

                MissionDefinition definition =
                    new MissionDefinition
                    {
                        id = saved.id,
                        title = saved.title,
                        requester = saved.requester,
                        description = saved.description,
                        objectiveType =
                            (MissionObjectiveType)saved.objectiveType,
                        targetServing =
                            !string.IsNullOrEmpty(
                                saved.targetServingName) &&
                            InventoryManager.Instance != null &&
                            InventoryManager.Instance.itemIndex != null
                                ? InventoryManager.Instance.itemIndex
                                    .GetItemFromString(
                                        saved.targetServingName
                                    )
                                : null,
                        targetName = saved.targetName,
                        targetAmount = saved.targetAmount,
                        startHour = saved.startHour,
                        endHour = saved.endHour,
                        rewardCoins = saved.rewardCoins,
                        rewardReputation = saved.rewardReputation
                    };

                missions.Add(
                    new MissionInstance
                    {
                        Definition = definition,
                        Progress = saved.progress,
                        State = (MissionState)saved.state
                    }
                );
            }
        }

        if (missions.Count == 0)
        {
            GameTimestamp timestamp =
                TimeManager.Instance != null
                    ? TimeManager.Instance.GetGameTimestamp()
                    : null;

            if (timestamp != null)
                StartDay(timestamp);

            return;
        }

        missionsReady = true;
        CheckTrustRewards();
    }

    public string GetSummaryText()
    {
        if (!missionsReady)
            return "Requests unlock after the tutorial.";

        return
            "Requests completed: " +
            completedToday +
            "/" +
            missions.Count +
            "  |  Trust: " +
            Reputation;
    }

    public bool AnyActiveMission()
    {
        foreach (MissionInstance mission in missions)
        {
            if (mission.IsActive)
                return true;
        }

        return false;
    }

    public bool HasGrowingCrops()
    {
        if (LandManager.Instance == null)
            return false;

        foreach (CropBehaviour crop in
                 LandManager.Instance.GetActiveCrops())
        {
            if (crop == null)
                continue;

            if (crop.cropState ==
                    CropBehaviour.CropState.Seed ||
                crop.cropState ==
                    CropBehaviour.CropState.Seedling)
            {
                return true;
            }
        }

        return false;
    }

    public int ActiveCustomerCount()
    {
        CustomerSpawner spawner =
            FindFirstObjectByType<CustomerSpawner>();

        return spawner != null
            ? spawner.GetCurrentCustomers()
            : 0;
    }

    static void Notify(string message)
    {
        if (NotificationManager.Instance != null)
        {
            NotificationManager.Instance.ShowMessage(
                message
            );
        }
    }
}
