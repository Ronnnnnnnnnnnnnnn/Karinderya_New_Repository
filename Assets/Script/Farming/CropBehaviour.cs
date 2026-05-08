using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CropBehaviour : MonoBehaviour
{
   SeedData seedToGrow;

   [Header("Stages of Life")]

    public GameObject seed;
    private GameObject seedling;
    private GameObject harvestable;

    float growth;

    float maxGrowth;

   public enum CropState
   {
    Seed, Seedling, Harvestable
   }

    public CropState cropState;

   public void Plant(SeedData seedToGrow)
    {
        this.seedToGrow = seedToGrow;

        seedling = Instantiate(seedToGrow.seedling, transform);

        ItemData cropToYield = seedToGrow.cropToYield;

        harvestable = Instantiate(cropToYield.gameModel, transform);

        float hoursToGrow = GameTimestamp.DaysToHours(seedToGrow.daysToGrow);

        maxGrowth = (int)GameTimestamp.HoursToMinutes(hoursToGrow);

        SwitchState(CropState.Seed);

    }

    public void Grow()
    {
        growth++;

        if(growth >= maxGrowth / 2 && cropState == CropState.Seed)
        {
            SwitchState(CropState.Seedling);
        }

        if(growth >= maxGrowth && cropState == CropState.Seedling)
        {
            SwitchState(CropState.Harvestable);
        }
    }

    void SwitchState(CropState stateToSwitch)
    {

        seed.SetActive(false);
        seedling.SetActive(false);
        harvestable.SetActive(false);

        switch(stateToSwitch)
        {
            case CropState.Seed:
            seed.SetActive(true);
            break;

            case CropState.Seedling:
            seedling.SetActive(true);
            break;

            case CropState.Harvestable:
            harvestable.SetActive(true);
            harvestable.transform.parent = null;
            Destroy(gameObject);
            break;
        }

        cropState = stateToSwitch;
    }
}
