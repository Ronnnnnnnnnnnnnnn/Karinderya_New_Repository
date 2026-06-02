using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CropBehaviour : MonoBehaviour
{
    int landID;

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

    [Header("Real-Time Growth")]
    public float maxGrowSeconds = 180f;

    float plantRealTime;
    bool useRealTimeGrowth = true;

    public SeedData SeedToGrow => seedToGrow;

   public void Plant(int landID, SeedData seedToGrow)
    {
        LoadCrop(landID, seedToGrow, CropState.Seed, 0, 0);
        LandManager.Instance.RegisterCrop(landID, seedToGrow, cropState, growth, health); 
    }

        public void LoadCrop(int landID, SeedData seedToGrow, CropState cropState, float growth, float health)
    {
        this.landID = landID;
        this.seedToGrow = seedToGrow;

        seedling = Instantiate(seedToGrow.seedling, transform);

        ItemData cropToYield = seedToGrow.cropToYield;

        harvestable = Instantiate(cropToYield.gameModel, transform);

        float hoursToGrow = GameTimestamp.DaysToHours(seedToGrow.daysToGrow);
        maxGrowth = GameTimestamp.HoursToMinutes(hoursToGrow);

        this.growth = growth;
        this.health = health;
        plantRealTime = Time.time;

        if (seedToGrow.regrowable)
        {
            RegrowableHarvestBehaviour regrowableHarvest = harvestable.GetComponent<RegrowableHarvestBehaviour>();

            regrowableHarvest.SetParent(this);
        }

        SwitchState(cropState);
    }

    void Update()
    {
        if (!useRealTimeGrowth || seedToGrow == null)
            return;

        if (cropState == CropState.Harvestable || cropState == CropState.Wilted)
            return;

        float progress = Mathf.Clamp01((Time.time - plantRealTime) / maxGrowSeconds);

        if (progress >= 1f && cropState != CropState.Harvestable)
        {
            SwitchState(CropState.Harvestable);
            LandManager.Instance.OnCropStateChange(landID, cropState, growth, health);
            return;
        }

        CropState targetState = CropState.Seed;
        if (progress >= 0.5f)
            targetState = CropState.Seedling;

        if (targetState != cropState && cropState != CropState.Harvestable)
            SwitchState(targetState);
    }

    public float GetRemainingGrowSeconds()
    {
        if (cropState == CropState.Harvestable)
            return 0f;

        if (!useRealTimeGrowth)
        {
            float remaining = maxGrowth - growth;
            return Mathf.Clamp(remaining * (maxGrowSeconds / Mathf.Max(maxGrowth, 1f)), 0f, maxGrowSeconds);
        }

        float elapsed = Time.time - plantRealTime;
        return Mathf.Clamp(maxGrowSeconds - elapsed, 0f, maxGrowSeconds);
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

        LandManager.Instance.OnCropStateChange(landID, cropState, growth, health);
    }

    public void Wither()
    {
        health--;

        if(health <= 0 && cropState != CropState.Seed)
        {
            SwitchState(CropState.Wilted);
        }

        LandManager.Instance.OnCropStateChange(landID, cropState, growth, health);
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
                RemoveCrop();
            }
            break;

            case CropState.Wilted:
            wilted.SetActive(true);
            break;
        }

        cropState = stateToSwitch;
    }

    public void RemoveCrop()
    {
        LandManager.Instance.DeregisterCrop(landID);
        Destroy(gameObject);
    }

    public void Regrow()
    {
        float hoursToRegrow = GameTimestamp.DaysToHours(seedToGrow.daysToRegrow);

        growth = maxGrowth - GameTimestamp.HoursToMinutes(hoursToRegrow);

        SwitchState(CropState.Seedling);
    }
}
