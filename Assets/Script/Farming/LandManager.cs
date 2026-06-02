using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandManager : MonoBehaviour
{
    public static LandManager Instance { get; private set; } 

    public static Tuple<List<LandSaveState>, List<CropSaveState>> farmData = null; 

    List<Land> landPlots = new List<Land>();

    List<LandSaveState> landData = new List<LandSaveState>();
    List<CropSaveState> cropData = new List<CropSaveState>(); 

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
    }


    private void OnDestroy()
    {
        farmData = new Tuple<List<LandSaveState>, List<CropSaveState>>(landData, cropData);        
    }
#region Registering and Deregistering
    void RegisterLandPlots()
    {
        foreach(Transform landTransform in transform)
        {
            Land land = landTransform.GetComponent<Land>();
            landPlots.Add(land);

            landData.Add(new LandSaveState());

            land.id = landPlots.Count - 1;
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
        
    }
#endregion
#region Loading Data
    public void ImportLandData(List<LandSaveState> landDatasetToLoad)
    {
        for(int i =0; i < landDatasetToLoad.Count; i++)
        {
            LandSaveState landDataToLoad = landDatasetToLoad[i];
            landPlots[i].LoadLandData(landDataToLoad.landStatus, landDataToLoad.lastWatered);
            
        }

        landData = landDatasetToLoad; 
    }

       public void ImportCropData(List<CropSaveState> cropDatasetToLoad)
    {
        cropData = cropDatasetToLoad;
        foreach (CropSaveState cropSave in cropDatasetToLoad)
        {
            Land landToPlant = landPlots[cropSave.landID];
            CropBehaviour cropToPlant = landToPlant.SpawnCrop();
            Debug.Log(cropToPlant.gameObject); 
            SeedData seedToGrow = (SeedData) InventoryManager.Instance.itemIndex.GetItemFromString(cropSave.seedToGrow);
            cropToPlant.LoadCrop(cropSave.landID, seedToGrow, cropSave.cropState, cropSave.growth, cropSave.health); 
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