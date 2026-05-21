using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Land : MonoBehaviour, ITimeTracker
{
<<<<<<< Updated upstream
    public enum LandStatus { Soil, Farmland, Watered }

    public LandStatus landStatus = LandStatus.Soil;

    public Material soilMat, farmlandMat, wateredMat;

    new Renderer renderer;
=======
    public enum LandStatus
    {
        Soil,
        Farmland,
        Watered
    }

    [Header("Land")]
    public LandStatus landStatus;

    public Material soilMat;
    public Material farmlandMat;
    public Material wateredMat;
>>>>>>> Stashed changes

    private Renderer landRenderer;

    [Header("Selection")]
    public GameObject select;

    private GameTimestamp timeWatered;

    [Header ("Crops")]
    
    public GameObject cropPrefab;

    private CropBehaviour cropPlanted = null;

    // =========================
    // START
    // =========================

    void Start()
    {
<<<<<<< Updated upstream
        renderer = GetComponent<Renderer>();
        SwitchLandStatus(LandStatus.Soil);
        if (select != null) select.SetActive(false);
=======
        // GET RENDERER
        landRenderer = GetComponent<Renderer>();

        // SAFETY CHECK
        if(landRenderer == null)
        {
            Debug.LogError(
                gameObject.name +
                " has no Renderer!"
            );

            return;
        }

        // START AS SOIL
        SwitchLandStatus(LandStatus.Soil);

        // HIDE SELECT
        Select(false);
>>>>>>> Stashed changes

        // REGISTER TO TIME MANAGER
        if(TimeManager.Instance != null)
        {
            TimeManager.Instance.RegisterTracker(this);
        }
        else
        {
            Debug.LogWarning(
                "TimeManager missing in scene!"
            );
        }
    }

    // =========================
    // LAND MATERIALS
    // =========================

    public void SwitchLandStatus(LandStatus statusToSwitch)
    {
        landStatus = statusToSwitch;

<<<<<<< Updated upstream
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
=======
        Material materialToSwitch = soilMat;

        switch(statusToSwitch)
        {
            case LandStatus.Soil:

                materialToSwitch = soilMat;

                break;

            case LandStatus.Farmland:

                materialToSwitch = farmlandMat;

                break;

            case LandStatus.Watered:

                materialToSwitch = wateredMat;

                // SAVE WATER TIME
                if(TimeManager.Instance != null)
                {
                    timeWatered =
                        TimeManager.Instance.GetGameTimestamp();
                }

                break;
        }

        // APPLY MATERIAL
        if(materialToSwitch != null)
        {
            landRenderer.material = materialToSwitch;
        }
>>>>>>> Stashed changes
    }

    // =========================
    // SELECTION VISUAL
    // =========================

    public void Select(bool toggle)
    {
<<<<<<< Updated upstream
        if (select != null)
            select.SetActive(toggle);
=======
        if(select != null)
        {
            select.SetActive(toggle);
        }
>>>>>>> Stashed changes
    }

    // =========================
    // PLAYER INTERACTION
    // =========================

    public void Interact()
    {
<<<<<<< Updated upstream
        ItemData toolSlots = InventoryManager.Instance.GetEquippedSlotItem(InventorySlot.InventoryType.Tool);

        if(toolSlots == null)
=======
        // NO TOOL EQUIPPED
        if(!InventoryManager.Instance.SlotEquipped(
            InventorySlot.InventoryType.Tool))
>>>>>>> Stashed changes
        {
            return;
        }

<<<<<<< Updated upstream
        EquipmentData equipmentTool = toolSlots as EquipmentData;
=======
        // GET TOOL
        ItemData toolSlot =
            InventoryManager.Instance.GetEquippedSlotItem(
                InventorySlot.InventoryType.Tool
            );

        if(toolSlot == null)
        {
            return;
        }

        // TOOL TYPE
        EquipmentData equipmentTool =
            toolSlot as EquipmentData;

        // =========================
        // EQUIPMENT TOOLS
        // =========================
>>>>>>> Stashed changes

        if(equipmentTool != null)
        {
            EquipmentData.ToolType toolType =
                equipmentTool.toolType;

            switch(toolType)
            {
                case EquipmentData.ToolType.Hoe:

<<<<<<< Updated upstream
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
=======
                    SwitchLandStatus(
                        LandStatus.Farmland
                    );

                    break;

                case EquipmentData.ToolType.WateringCan:

                    SwitchLandStatus(
                        LandStatus.Watered
                    );

                    break;

                case EquipmentData.ToolType.Shovel:

                    if(cropPlanted != null)
                    {
                        Destroy(cropPlanted.gameObject);

                        cropPlanted = null;
                    }

                    break;
            }

            return;
        }

        // =========================
        // SEEDS
        // =========================
>>>>>>> Stashed changes

        SeedData seedTool = toolSlot as SeedData;

        if(seedTool != null &&
           landStatus != LandStatus.Soil &&
           cropPlanted == null)
        {
<<<<<<< Updated upstream
            GameObject cropObject = Instantiate(cropPrefab, transform);

            cropObject.transform.position = new Vector3(transform.position.x, 0f, transform.position.z);

            cropPlanted = cropObject.GetComponent<CropBehaviour>();

            cropPlanted.Plant(seedTool);
=======
            // SPAWN CROP
            GameObject cropObject =
                Instantiate(cropPrefab, transform);

            cropObject.transform.position =
                new Vector3(
                    transform.position.x,
                    0,
                    transform.position.z
                );

            // GET CROP SCRIPT
            cropPlanted =
                cropObject.GetComponent<CropBehaviour>();

            // SAFETY CHECK
            if(cropPlanted != null)
            {
                cropPlanted.Plant(seedTool);

                // CONSUME SEED
                InventoryManager.Instance.ConsumeItem(
                    InventoryManager.Instance.GetEquippedSlot(
                        InventorySlot.InventoryType.Tool
                    )
                );
            }
            else
            {
                Debug.LogError(
                    "Crop prefab missing CropBehaviour!"
                );
            }
>>>>>>> Stashed changes
        }
    }

    // =========================
    // TIME UPDATE
    // =========================

    public void ClockUpdate(GameTimestamp timestamp)
    {
        // WATERED LAND
        if(landStatus == LandStatus.Watered)
        {
<<<<<<< Updated upstream
            float hoursElapsed = GameTimestamp.CompareTimestamps(timeWatered, timestamp);
            Debug.Log(hoursElapsed + "hours since this was watered");
=======
            float hoursElapsed =
                GameTimestamp.CompareTimestamps(
                    timeWatered,
                    timestamp
                );
>>>>>>> Stashed changes

            Debug.Log(
                hoursElapsed +
                " hours since watered"
            );

            // GROW CROP
            if(cropPlanted != null)
            {
                cropPlanted.Grow();
            }

<<<<<<< Updated upstream
            if(hoursElapsed > 1.5)
=======
            // DRY LAND
            if(hoursElapsed > 3)
>>>>>>> Stashed changes
            {
                SwitchLandStatus(
                    LandStatus.Farmland
                );
            }
        }

        // WITHER CROP
        if(landStatus != LandStatus.Watered &&
           cropPlanted != null)
        {
<<<<<<< Updated upstream
            if(cropPlanted.cropState != CropBehaviour.CropState.Seed)
=======
            if(cropPlanted.cropState !=
               CropBehaviour.CropState.Seed)
>>>>>>> Stashed changes
            {

            }
        }
    }
}