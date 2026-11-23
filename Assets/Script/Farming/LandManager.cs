using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandManager : MonoBehaviour
{
    public static LandManager Instance { get; private set; }

    public static Tuple<List<LandSaveState>, List<CropSaveState>> farmData = null;

    [Header("Farm Size")]
    [Min(1)]
    public int maxActivePlots = 16;

    public bool IsLoaded { get; private set; }

    List<Land> landPlots = new List<Land>();

    List<LandSaveState> landData = new List<LandSaveState>();
    List<CropSaveState> cropData = new List<CropSaveState>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        Instance = null;
        farmData = null;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    void OnEnable()
    {
        if (farmData == null && SaveManager.HasSave)
        {
            farmData = SaveManager.CreateFarmDataFromSave();
        }

        RegisterLandPlots();
        StartCoroutine(LoadFarmData());
    }

    IEnumerator LoadFarmData()
    {
        yield return new WaitForEndOfFrame();
        if (farmData != null)
        {
            ImportLandData(farmData.Item1);
            ImportCropData(farmData.Item2);
        }

        IsLoaded = true;
    }

    private void OnDestroy()
    {
        farmData = new Tuple<List<LandSaveState>, List<CropSaveState>>(landData, cropData);
    }
#region Registering and Deregistering
    void RegisterLandPlots()
    {
        List<Land> allPlots = new List<Land>();

        foreach (Transform landTransform in transform)
        {
            Land land = landTransform.GetComponent<Land>();

            if (land != null)
                allPlots.Add(land);
        }

        allPlots.Sort(
            (a, b) =>
            {
                float zA = a.transform.localPosition.z;
                float zB = b.transform.localPosition.z;
                return zB.CompareTo(zA);
            }
        );

        int activeCount = Mathf.Clamp(
            maxActivePlots,
            1,
            Mathf.Max(1, allPlots.Count)
        );

        for (int i = 0; i < allPlots.Count; i++)
        {
            Land land = allPlots[i];

            if (i >= activeCount)
            {
                land.gameObject.SetActive(false);
                continue;
            }

            landPlots.Add(land);
            landData.Add(new LandSaveState());
            land.id = landPlots.Count - 1;
        }
    }

    public void WriteSave(SaveData data)
    {
        if (data == null)
            return;

        data.land = new List<LandSlotSave>();
        data.crops = new List<CropSlotSave>();

        foreach (LandSaveState land in landData)
        {
            GameTimestamp watered = land.lastWatered;

            data.land.Add(
                new LandSlotSave
                {
                    status = (int)land.landStatus,
                    year = watered != null ? watered.year : 0,
                    season = watered != null ? (int)watered.season : 0,
                    day = watered != null ? watered.day : 0,
                    hour = watered != null ? watered.hour : 0,
                    minute = watered != null ? watered.minute : 0
                }
            );
        }

        foreach (CropSaveState crop in cropData)
        {
            data.crops.Add(
                new CropSlotSave
                {
                    landID = crop.landID,
                    seedName = crop.seedToGrow,
                    cropState = (int)crop.cropState,
                    growth = crop.growth,
                    health = crop.health
                }
            );
        }
    }

    public void RegisterCrop(int landID, SeedData seedToGrow, CropBehaviour.CropState cropState, float growth, float health)
    {
        cropData.Add(new CropSaveState(landID, seedToGrow.name, cropState, growth, health));
    }

    public void DeregisterCrop(int landID)
    {
        cropData.RemoveAll(x => x.landID == landID);
    }
#endregion

#region State Changes

    public void OnLandStateChange(int id, Land.LandStatus landStatus, GameTimestamp lastWatered)
    {
        landData[id] = new LandSaveState(landStatus, lastWatered);
    }

    public void OnCropStateChange(int landID, CropBehaviour.CropState cropState, float growth, float health)
    {
        for (int i = 0; i < cropData.Count; i++)
        {
            if (cropData[i].landID == landID)
            {
                cropData[i] = new CropSaveState(landID, cropData[i].seedToGrow, cropState, growth, health);
                return;
            }
        }
    }
#endregion
#region Loading Data
    public void ImportLandData(List<LandSaveState> landDatasetToLoad)
    {
        if (landDatasetToLoad == null)
            return;

        int count = Mathf.Min(
            landDatasetToLoad.Count,
            landPlots.Count
        );

        for (int i = 0; i < count; i++)
        {
            LandSaveState landDataToLoad = landDatasetToLoad[i];
            landPlots[i].LoadLandData(landDataToLoad.landStatus, landDataToLoad.lastWatered);
        }

        landData = new List<LandSaveState>();

        for (int i = 0; i < landPlots.Count; i++)
        {
            landData.Add(
                i < landDatasetToLoad.Count
                    ? landDatasetToLoad[i]
                    : new LandSaveState()
            );
        }
    }

    public void ImportCropData(List<CropSaveState> cropDatasetToLoad)
    {
        if (cropDatasetToLoad == null)
            return;

        cropData = new List<CropSaveState>();

        foreach (CropSaveState cropSave in new List<CropSaveState>(cropDatasetToLoad))
        {
            if (cropSave.landID < 0 ||
                cropSave.landID >= landPlots.Count)
            {
                continue;
            }

            Land landToPlant = landPlots[cropSave.landID];
            SeedData seedToGrow = InventoryManager.Instance.itemIndex.GetItemFromString(cropSave.seedToGrow) as SeedData;

            if (seedToGrow != null)
            {
                cropData.Add(cropSave);
                CropBehaviour cropToPlant = landToPlant.SpawnCrop();
                cropToPlant.LoadCrop(cropSave.landID, seedToGrow, cropSave.cropState, cropSave.growth, cropSave.health);
            }
        }
    }
#endregion

    public List<CropBehaviour> GetActiveCrops()
    {
        var crops = new List<CropBehaviour>();

        foreach (Land land in landPlots)
        {
            CropBehaviour crop = land.GetPlantedCrop();
            if (crop != null)
                crops.Add(crop);
        }

        return crops;
    }
}
