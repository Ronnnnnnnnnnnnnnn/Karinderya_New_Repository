using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Land : MonoBehaviour, ITimeTracker
{
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

    [Header("Plant Timer UI")]
    public TextMeshProUGUI timerText;

    SeedData plantedSeed;
    GameTimestamp plantTime;

    void Start()
    {
        renderer = GetComponent<Renderer>();

        SwitchLandStatus(LandStatus.Soil);

        Select(false);

        TimeManager.Instance.RegisterTracker(this);
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
                    SwitchLandStatus(LandStatus.Watered);
                    break;

                case EquipmentData.ToolType.Shovel:

                    if(cropPlanted != null)
                    {
                        Destroy(cropPlanted.gameObject);
                    }
                    break; 
            }

            return; 
        }

        SeedData seedTool = toolSlot as SeedData; 

        if(seedTool != null && landStatus == LandStatus.Farmland && cropPlanted == null)
        {
            GameObject cropObject = Instantiate(cropPrefab, transform);
            cropObject.transform.localPosition =  new Vector3(0f, 0.5f, 0f);

            cropPlanted = cropObject.GetComponent<CropBehaviour>();
            cropPlanted.Plant(seedTool);

            plantedSeed = seedTool;
            plantTime = TimeManager.Instance.GetGameTimestamp();

            InventoryManager.Instance.ConsumeItem(InventoryManager.Instance.GetEquippedSlot(InventorySlot.InventoryType.Tool));

        }
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

        if (timerText != null)
        {
            timerText.text = plantedSeed != null ? GetRemainingTime() : "";
        }
    }

    public string GetRemainingTime()
    {
        if (plantedSeed == null)
            return "";

        float daysPassed = GameTimestamp.CompareTimestamps(
            plantTime,
            TimeManager.Instance.GetGameTimestamp()
        );

        float remaining = plantedSeed.daysToGrow - daysPassed;

        if (remaining <= 0)
            return "Fully Grown!";

        return Mathf.CeilToInt(remaining * 24) + " hours left";
    }
}