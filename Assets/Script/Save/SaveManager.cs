using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }
    public static SaveData Data { get; private set; }
    public static bool HasSave { get; private set; }

    public static bool HasCompletedTutorial =>
        HasSave &&
        Data != null &&
        Data.tutorialCompleted;

    public static string SavePath =>
        Path.Combine(
            Application.persistentDataPath,
            "karinderya_save.json"
        );

    public float autosaveInterval = 20f;

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        Instance = null;
        Data = null;
        HasSave = false;
    }

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Preload()
    {
        LoadFromDisk();
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        StartCoroutine(AutosaveLoop());
    }

    void OnApplicationQuit()
    {
        SaveCurrent();
    }

    void OnApplicationPause(bool paused)
    {
        if (paused)
            SaveCurrent();
    }

    IEnumerator AutosaveLoop()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(
                autosaveInterval
            );

            SaveCurrent();
        }
    }

    public static void SaveCurrent()
    {
        if (!Application.isPlaying)
            return;

        if (LandManager.Instance != null &&
            !LandManager.Instance.IsLoaded)
        {
            return;
        }

        SaveData snapshot = Capture();

        if (snapshot == null)
            return;

        Data = snapshot;
        HasSave = true;
        WriteToDisk(snapshot);
    }

    public static SaveData Capture()
    {
        SaveData data = new SaveData();

        TutorialManager tutorial =
            UnityEngine.Object.FindFirstObjectByType<TutorialManager>();

        data.tutorialCompleted =
            tutorial == null || tutorial.IsFinished;

        TimeManager timeManager =
            UnityEngine.Object.FindFirstObjectByType<TimeManager>();

        if (timeManager != null)
        {
            GameTimestamp timestamp =
                timeManager.GetGameTimestamp();

            if (timestamp != null)
            {
                data.year = timestamp.year;
                data.season = (int)timestamp.season;
                data.day = timestamp.day;
                data.hour = timestamp.hour;
                data.minute = timestamp.minute;
            }

            data.timeScale = timeManager.timeScale;
        }

        if (CurrencyManager.Instance != null)
            data.coins = CurrencyManager.Instance.coins;

        if (PlayerProgression.Instance != null)
            data.dishesServed =
                PlayerProgression.Instance.dishesServed;

        if (InventoryManager.Instance != null)
            InventoryManager.Instance.WriteSave(data);

        if (RecipeUnlockManager.Instance != null)
            RecipeUnlockManager.Instance.WriteSave(data);

        if (LandManager.Instance != null)
            LandManager.Instance.WriteSave(data);

        BuffetContainer[] buffets =
            UnityEngine.Object.FindObjectsByType<BuffetContainer>(
                FindObjectsSortMode.None
            );

        foreach (BuffetContainer buffet in buffets)
        {
            if (buffet == null)
                continue;

            data.buffets.Add(buffet.WriteSave());
        }

        if (MissionManager.Instance != null)
            MissionManager.Instance.WriteSave(data);

        return data;
    }

    public static void LoadFromDisk()
    {
        Data = null;
        HasSave = false;

        if (!File.Exists(SavePath))
            return;

        try
        {
            string json = File.ReadAllText(SavePath);
            SaveData loaded = JsonUtility.FromJson<SaveData>(json);

            if (loaded == null)
                return;

            Data = loaded;
            HasSave = true;
        }
        catch (Exception exception)
        {
            Debug.LogWarning(
                "[SAVE] Could not load save: " +
                exception.Message
            );
        }
    }

    static void WriteToDisk(SaveData data)
    {
        try
        {
            string directory =
                Path.GetDirectoryName(SavePath);

            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(SavePath, json);
        }
        catch (Exception exception)
        {
            Debug.LogWarning(
                "[SAVE] Could not write save: " +
                exception.Message
            );
        }
    }

    public static void DeleteSave()
    {
        if (File.Exists(SavePath))
            File.Delete(SavePath);

        Data = null;
        HasSave = false;
    }

    public static Tuple<
        List<LandSaveState>,
        List<CropSaveState>> CreateFarmDataFromSave()
    {
        if (Data == null)
            return null;

        List<LandSaveState> landData =
            new List<LandSaveState>();

        foreach (LandSlotSave saved in Data.land)
        {
            GameTimestamp watered =
                new GameTimestamp(
                    saved.year,
                    (GameTimestamp.Season)saved.season,
                    saved.day,
                    saved.hour,
                    saved.minute
                );

            landData.Add(
                new LandSaveState(
                    (Land.LandStatus)saved.status,
                    watered
                )
            );
        }

        List<CropSaveState> cropData =
            new List<CropSaveState>();

        foreach (CropSlotSave saved in Data.crops)
        {
            cropData.Add(
                new CropSaveState(
                    saved.landID,
                    saved.seedName,
                    (CropBehaviour.CropState)saved.cropState,
                    saved.growth,
                    saved.health
                )
            );
        }

        return new Tuple<
            List<LandSaveState>,
            List<CropSaveState>>(
                landData,
                cropData
            );
    }

    public static BuffetSlotSave FindBuffetSave(
        string recipeName)
    {
        if (Data == null)
            return null;

        foreach (BuffetSlotSave saved in Data.buffets)
        {
            if (saved.recipeName == recipeName)
                return saved;
        }

        return null;
    }
}
