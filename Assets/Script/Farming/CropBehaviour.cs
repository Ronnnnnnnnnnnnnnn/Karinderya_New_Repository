using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CropBehaviour : MonoBehaviour
{
   SeedData seedToGrow;

   [Header("Stages of Life")]

    public GameObject seed;
    public GameObject wilted;
    private GameObject seedling;
    private GameObject harvestable;

    float growth;

    float maxGrowth;

    float maxHealth = GameTimestamp.HoursToMinutes(3);

    float health;

   public enum CropState
   {
    Seed, Seedling, Harvestable, Wilted
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

        if(seedToGrow.regrowable)
        {
            RegrowableHarvestBehaviour regrowableHarvest = harvestable.GetComponent<RegrowableHarvestBehaviour>();

            regrowableHarvest.SetParent(this);
        }

        SwitchState(CropState.Seed);

    }

    public void Grow()
    {
        growth++;

        if(health < maxHealth)
        {
            health++;
        }

        if(growth >= maxGrowth / 2 && cropState == CropState.Seed)
        {
            SwitchState(CropState.Seedling);
        }

        if(growth >= maxGrowth && cropState == CropState.Seedling)
        {
            SwitchState(CropState.Harvestable);
        }
    }

    public void Wilted()
    {
        health--;

        if(health <= 0 && cropState != CropState.Seed)
        {
            SwitchState(CropState.Wilted);
        }
    }

    void SwitchState(CropState stateToSwitch)
    {

        seed.SetActive(false);
        seedling.SetActive(false);
        harvestable.SetActive(false);
        wilted.SetActive(false);

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

            if(!seedToGrow.regrowable)
            {
                harvestable.transform.parent = null;
                Destroy(gameObject);
            }
            break;

            case CropState.Wilted:
            wilted.SetActive(true);
            break;
        }

        cropState = stateToSwitch;
    }

    public void Regrow()
    {
        float hoursToRegrow = GameTimestamp.DaysToHours(seedToGrow.daysToRegrow);

        growth = maxGrowth - GameTimestamp.HoursToMinutes(hoursToRegrow);

        SwitchState(CropState.Seedling);
    }
}
