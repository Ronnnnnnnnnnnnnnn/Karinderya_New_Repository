using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Land : MonoBehaviour, ITimeTracker
{
    public int id; 
    public enum LandStatus
    {
        Soil, Farmland, Watered
    }

    public LandStatus landStatus;

    public Material soilMat, farmlandMat, wateredMat;
    new Renderer renderer;

    public GameObject select;

    GameTimestamp timeWatered;

    [Header("Crops")]
    public GameObject cropPrefab;

    CropBehaviour cropPlanted = null;

    [Header("Water Timer UI")]
    public TextMeshProUGUI timerText;
    public GameObject timerUI;

    SeedData plantedSeed;
    GameTimestamp plantTime;

    void Start()
    {
        renderer = GetComponent<Renderer>();

        SwitchLandStatus(LandStatus.Soil);

        Select(false);

        TimeManager.Instance.RegisterTracker(this);
    }

    public void LoadLandData(LandStatus statusToSwitch, GameTimestamp lastWatered)
    {
        landStatus = statusToSwitch;
        timeWatered = lastWatered;

        Material materialToSwitch = soilMat;

        switch (statusToSwitch)
        {
            case LandStatus.Soil:
                materialToSwitch = soilMat;
                break;
            case LandStatus.Farmland:
                materialToSwitch = farmlandMat;
                break;

            case LandStatus.Watered:
                materialToSwitch = wateredMat;
                break;

        }
        renderer.material = materialToSwitch;
    }

    public void SwitchLandStatus(LandStatus statusToSwitch)
    {
        landStatus = statusToSwitch;

        Material materialToSwitch = soilMat; 

        switch (statusToSwitch)
        {
            case LandStatus.Soil:
                materialToSwitch = soilMat;
                break;
            case LandStatus.Farmland:
                materialToSwitch = farmlandMat;
                break;

            case LandStatus.Watered:
                materialToSwitch = wateredMat;

                timeWatered = TimeManager.Instance.GetGameTimestamp(); 
                break; 

        }

        renderer.material = materialToSwitch; 

        LandManager.Instance.OnLandStateChange(id, landStatus, timeWatered);
    }

    public void Select(bool toggle)
    {
        select.SetActive(toggle);
    }

    public void Interact()
    {
        ItemData toolSlot = InventoryManager.Instance.GetEquippedSlotItem(InventorySlot.InventoryType.Tool);

        if (!InventoryManager.Instance.SlotEquipped(InventorySlot.InventoryType.Tool))
        {
            return; 
        }

        EquipmentData equipmentTool = toolSlot as EquipmentData; 

        if(equipmentTool != null)
        {
            EquipmentData.ToolType toolType = equipmentTool.toolType;

            switch (toolType)
            {
                case EquipmentData.ToolType.Hoe:
                    SwitchLandStatus(LandStatus.Farmland);
                    break;
                case EquipmentData.ToolType.WateringCan:

                if(landStatus != LandStatus.Soil)
                {
                    SwitchLandStatus(LandStatus.Watered);
                }  
                    break;

                case EquipmentData.ToolType.Shovel:

                    if(cropPlanted != null)
                    {
                        cropPlanted.RemoveCrop();
                    }
                    break; 
            }

            return; 
        }

        SeedData seedTool = toolSlot as SeedData; 

        if(seedTool != null && landStatus == LandStatus.Farmland && cropPlanted == null)
        {
            SpawnCrop();

            cropPlanted.Plant(id, seedTool);

            InventoryManager.Instance.ConsumeItem(InventoryManager.Instance.GetEquippedSlot(InventorySlot.InventoryType.Tool));

        }
    }

    public CropBehaviour SpawnCrop()
    {
        GameObject cropObject = Instantiate(cropPrefab, transform);
        cropObject.transform.position = new Vector3(transform.position.x, 0.150f, transform.position.z);

        cropPlanted = cropObject.GetComponent<CropBehaviour>();
        return cropPlanted; 
    }

    public void ClockUpdate(GameTimestamp timestamp)
    {
        if(landStatus == LandStatus.Watered)
        {
            float hoursElapsed = GameTimestamp.CompareTimestamps(timeWatered, timestamp);
            Debug.Log(hoursElapsed + " hours since this was watered");

            if(cropPlanted != null)
            {
                cropPlanted.Grow();
            }

            if(hoursElapsed > 3)
            {
                SwitchLandStatus(LandStatus.Farmland);
            }
        }

        if(landStatus != LandStatus.Watered && cropPlanted != null)
        {
            if (cropPlanted.cropState != CropBehaviour.CropState.Seed)
            {
                cropPlanted.Wither();
            }
        }
    }

    public void ShowTimer(bool show)
    {
        if(timerUI != null)
            timerUI.SetActive(show);
    }

    public void UpdateTimerUI()
    {
        if (timerText == null) return;

        if (cropPlanted != null &&
            cropPlanted.cropState == CropBehaviour.CropState.Harvestable)
        {
            timerText.text = "Ready to Harvest!";
            return;
        }

        if (landStatus == LandStatus.Watered)
        {
            float elapsed =
                GameTimestamp.CompareTimestamps(
                    timeWatered,
                    TimeManager.Instance.GetGameTimestamp());

            float remaining = 4f - elapsed;

            if (remaining < 0)
                remaining = 0;

            timerText.text = "Water again in " + remaining.ToString("0.0") + "hours";
        }
        else
        {
            timerText.text = "";
        }
    }

    void Update()
    {
        if(timerUI != null && timerUI.activeSelf)
        {
            UpdateTimerUI();
        }
    }
}