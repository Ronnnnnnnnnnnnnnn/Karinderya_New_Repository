using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Land : MonoBehaviour, ITimeTracker
{
    public enum LandStatus { Soil, Farmland, Watered }

    public LandStatus landStatus = LandStatus.Soil;

    public Material soilMat, farmlandMat, wateredMat;

    new Renderer renderer;

    public GameObject select;

    GameTimestamp timeWatered;

    [Header ("Crops")]
    
    public GameObject cropPrefab;

    CropBehaviour cropPlanted = null;

    void Start()
    {
        renderer = GetComponent<Renderer>();
        SwitchLandStatus(LandStatus.Soil);
        if (select != null) select.SetActive(false);

        TimeManager.Instance.RegisterTracker(this);
    }

    public void SwitchLandStatus(LandStatus statusToSwitch)
    {
        landStatus = statusToSwitch;

        Material mat = soilMat;
        switch (statusToSwitch)
        {
            case LandStatus.Soil: 
            mat = soilMat; 
            break;
            case LandStatus.Farmland:
            mat = farmlandMat; 
            break;
            case LandStatus.Watered:
            mat = wateredMat; 
            timeWatered = TimeManager.Instance.GetGameTimestamp();
            break;
        }

        if (renderer != null && mat != null)
            renderer.material = mat;
    }

    public void Select(bool toggle)
    {
        if (select != null)
            select.SetActive(toggle);
    }

    public void Interact()
    {
        ItemData toolSlots = InventoryManager.Instance.GetEquippedSlotItem(InventorySlot.InventoryType.Tool);

        if(toolSlots == null)
        {
            return;
        }

        EquipmentData equipmentTool = toolSlots as EquipmentData;

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

        SeedData seedTool = toolSlots as SeedData;

        if(seedTool != null && landStatus != LandStatus.Soil && cropPlanted == null)
        {
            GameObject cropObject = Instantiate(cropPrefab, transform);

            cropObject.transform.position = new Vector3(transform.position.x, 0f, transform.position.z);

            cropPlanted = cropObject.GetComponent<CropBehaviour>();

            cropPlanted.Plant(seedTool);
        }
    }

    public void ClockUpdate(GameTimestamp timestamp)
    {
        if(landStatus == LandStatus.Watered)
        {
            float hoursElapsed = GameTimestamp.CompareTimestamps(timeWatered, timestamp);
            Debug.Log(hoursElapsed + "hours since this was watered");

            if(cropPlanted != null)
            {
                cropPlanted.Grow();
            }

            if(hoursElapsed > 1.5)
            {
                SwitchLandStatus(LandStatus.Farmland);
            }
        }

        if(landStatus != LandStatus.Watered && cropPlanted != null)
        {
            if(cropPlanted.cropState != CropBehaviour.CropState.Seed)
            {

            }
        }
    }
}