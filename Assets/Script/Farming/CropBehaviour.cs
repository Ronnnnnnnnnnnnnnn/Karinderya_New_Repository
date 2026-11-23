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
        Seed,
        Seedling,
        Harvestable,
        Wilted
    }

    public CropState cropState;

    [Header("Real-Time Growth")]
    public float maxGrowSeconds = 180f;

    float plantRealTime;
    float saveTimer;

    bool useRealTimeGrowth = false;

    public SeedData SeedToGrow => seedToGrow;

    public void Plant(int landID, SeedData seedToGrow)
    {

        LoadCrop(
            landID,
            seedToGrow,
            CropState.Seed,
            0,
            maxHealth / 3f
        );

        LandManager.Instance.RegisterCrop(
            landID,
            seedToGrow,
            cropState,
            growth,
            health
        );

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(
                AudioManager.Instance.plantSound
            );
        }
    }

    public void LoadCrop(
        int landID,
        SeedData seedToGrow,
        CropState cropState,
        float growth,
        float health)
    {
        this.landID = landID;
        this.seedToGrow = seedToGrow;

        seedling = Instantiate(
            seedToGrow.seedling,
            transform
        );

        ItemData cropToYield =
            seedToGrow.cropToYield;

        harvestable = Instantiate(
            cropToYield.gameModel,
            transform
        );

        float hoursToGrow =
            GameTimestamp.DaysToHours(
                seedToGrow.daysToGrow
            );

        maxGrowth =
            GameTimestamp.HoursToMinutes(
                hoursToGrow
            );

        float gameMinutesPerSecond =
            TimeManager.Instance != null
                ? Mathf.Max(0.01f, TimeManager.Instance.timeScale)
                : 1f;

        if (maxGrowth > 0f)
        {
            maxGrowSeconds = maxGrowth / gameMinutesPerSecond;
        }
        else
        {
            maxGrowSeconds = Mathf.Max(1f, maxGrowSeconds);
        }

        this.growth = growth;
        this.health = health;

        plantRealTime =
            Time.time - growth;

        if (seedToGrow.regrowable)
        {
            RegrowableHarvestBehaviour regrowableHarvest =
                harvestable.GetComponent<
                    RegrowableHarvestBehaviour
                >();

            if (regrowableHarvest != null)
            {
                regrowableHarvest.SetParent(this);
            }
            else
            {
                Debug.LogWarning(
                    "[CROP] " +
                    seedToGrow.name +
                    " is regrowable but its harvest model " +
                    "has no RegrowableHarvestBehaviour"
                );
            }
        }
        else
        {

            InteractableObject pickup =
                harvestable.GetComponent<
                    InteractableObject
                >();

            if (pickup != null)
            {
                pickup.onPickedUp = RemoveCrop;
            }
            else
            {
                Debug.LogWarning(
                    "[CROP] " +
                    seedToGrow.name +
                    " harvest model has no InteractableObject"
                );
            }
        }

        MatchHarvestableSize(cropToYield);

        PlaceHarvestableOnPlot();

        EnsureHarvestableHasCollider();

        SwitchState(cropState);
    }

    void MatchHarvestableSize(ItemData cropToYield)
    {
        ItemIndex index =
            InventoryManager.Instance != null
                ? InventoryManager.Instance.itemIndex
                : null;

        if (index == null || index.items == null)
        {
            return;
        }

        float sum = 0f;
        int count = 0;

        foreach (
            string referenceName
            in new[] { "Carrot", "Potato" })
        {
            ItemData reference =
                index.items.Find(
                    i =>
                        i != null &&
                        !(i is SeedData) &&
                        i.name == referenceName
                );

            if (reference == null ||
                reference.gameModel == null)
            {
                continue;
            }

            if (reference == cropToYield)
            {
                return;
            }

            float size =
                MeasureModelSize(
                    reference.gameModel
                );

            if (size > 0f)
            {
                sum += size;
                count++;
            }
        }

        if (count == 0)
        {
            return;
        }

        float targetSize =
            sum / count;

        float currentSize =
            MeasureSize(harvestable);

        if (currentSize <= 0f)
        {
            return;
        }

        float factor =
            targetSize / currentSize;

        if (factor <= 1.05f)
        {
            return;
        }

        Debug.Log(
            "[CROP] Scaling " +
            cropToYield.name +
            " ripe model x" +
            factor.ToString("0.0")
        );

        harvestable.transform.localScale *=
            factor;
    }

    float MeasureModelSize(GameObject prefab)
    {
        GameObject temp =
            Instantiate(
                prefab,
                transform
            );

        float size =
            MeasureSize(temp);

        temp.SetActive(false);

        DestroyImmediate(temp);

        return size;
    }

    static float MeasureSize(GameObject model)
    {
        Renderer[] renderers =
            model.GetComponentsInChildren<
                Renderer
            >(true);

        if (renderers.Length == 0)
        {
            return 0f;
        }

        Bounds bounds =
            renderers[0].bounds;

        for (int i = 1;
            i < renderers.Length;
            i++)
        {
            bounds.Encapsulate(
                renderers[i].bounds
            );
        }

        Vector3 s =
            bounds.size;

        return Mathf.Pow(
            Mathf.Max(
                s.x,
                0.0001f
            ) *
            Mathf.Max(
                s.y,
                0.0001f
            ) *
            Mathf.Max(
                s.z,
                0.0001f
            ),
            1f / 3f
        );
    }

    void PlaceHarvestableOnPlot()
    {
        Renderer[] renderers =
            harvestable.GetComponentsInChildren<
                Renderer
            >(true);

        if (renderers.Length == 0)
        {
            return;
        }

        Bounds bounds =
            renderers[0].bounds;

        for (int i = 1;
            i < renderers.Length;
            i++)
        {
            bounds.Encapsulate(
                renderers[i].bounds
            );
        }

        Vector3 origin =
            transform.position;

        if (Vector3.Distance(
                bounds.center,
                origin) <= 1.5f)
        {
            return;
        }

        Debug.LogWarning(
            "[CROP] " +
            harvestable.name +
            " model is offset from the plot by " +
            Vector3.Distance(
                bounds.center,
                origin
            ).ToString("0.0") +
            " units (size " +
            bounds.size +
            "). Moving it onto the plot."
        );

        harvestable.transform.position +=
            new Vector3(
                origin.x - bounds.center.x,
                origin.y - bounds.min.y,
                origin.z - bounds.center.z
            );
    }

    void EnsureHarvestableHasCollider()
    {
        if (harvestable.GetComponentInChildren<
                Collider
            >(true) != null)
        {
            return;
        }

        Renderer[] renderers =
            harvestable.GetComponentsInChildren<
                Renderer
            >(true);

        if (renderers.Length == 0)
        {
            return;
        }

        Bounds bounds =
            renderers[0].bounds;

        for (int i = 1;
            i < renderers.Length;
            i++)
        {
            bounds.Encapsulate(
                renderers[i].bounds
            );
        }

        BoxCollider box =
            harvestable.AddComponent<BoxCollider>();

        Vector3 scale =
            harvestable.transform.lossyScale;

        box.center =
            harvestable.transform.InverseTransformPoint(
                bounds.center
            );

        box.size =
            new Vector3(
                bounds.size.x /
                    Mathf.Max(
                        Mathf.Abs(scale.x),
                        0.0001f
                    ),

                bounds.size.y /
                    Mathf.Max(
                        Mathf.Abs(scale.y),
                        0.0001f
                    ),

                bounds.size.z /
                    Mathf.Max(
                        Mathf.Abs(scale.z),
                        0.0001f
                    )
            );
    }

    void Update()
    {
        if (!useRealTimeGrowth ||
            seedToGrow == null)
        {
            return;
        }

        if (cropState == CropState.Harvestable ||
            cropState == CropState.Wilted)
        {
            return;
        }

        float elapsed =
            Time.time - plantRealTime;

        growth = elapsed;

        float progress =
            Mathf.Clamp01(
                elapsed / maxGrowSeconds
            );

        saveTimer -= Time.deltaTime;

        if (saveTimer <= 0f)
        {
            saveTimer = 1f;

            LandManager.Instance.OnCropStateChange(
                landID,
                cropState,
                growth,
                health
            );
        }

        if (progress >= 1f &&
            cropState != CropState.Harvestable)
        {
            SwitchState(
                CropState.Harvestable
            );

            LandManager.Instance.OnCropStateChange(
                landID,
                cropState,
                growth,
                health
            );

            return;
        }

        CropState targetState =
            CropState.Seed;

        if (progress >= 0.5f)
        {
            targetState =
                CropState.Seedling;
        }

        if (targetState != cropState &&
            cropState != CropState.Harvestable)
        {
            SwitchState(targetState);
        }
    }

    public float GetRemainingGrowSeconds()
    {
        if (cropState == CropState.Harvestable)
        {
            return 0f;
        }

        if (!useRealTimeGrowth)
        {
            float remaining =
                maxGrowth - growth;

            float clockSpeed =
                TimeManager.Instance != null
                    ? Mathf.Max(
                        0.01f,
                        TimeManager.Instance.timeScale
                    )
                    : 1f;

            return Mathf.Clamp(
                remaining / clockSpeed,
                0f,
                maxGrowth / clockSpeed
            );
        }

        float elapsed =
            Time.time - plantRealTime;

        return Mathf.Clamp(
            maxGrowSeconds - elapsed,
            0f,
            maxGrowSeconds
        );
    }

    public void Grow()
    {
        if (health < maxHealth)
        {
            health++;
        }

        if (useRealTimeGrowth)
        {
            LandManager.Instance.OnCropStateChange(
                landID,
                cropState,
                growth,
                health
            );

            return;
        }

        growth++;

        if (growth >= maxGrowth / 2 &&
            cropState == CropState.Seed)
        {
            SwitchState(
                CropState.Seedling
            );
        }

        if (growth >= maxGrowth &&
            cropState == CropState.Seedling)
        {
            SwitchState(
                CropState.Harvestable
            );
        }

        LandManager.Instance.OnCropStateChange(
            landID,
            cropState,
            growth,
            health
        );

    }

    public void Wither()
    {
        health--;

        if (health <= 0 &&
            cropState != CropState.Seed &&
            cropState != CropState.Wilted)
        {
            SwitchState(
                CropState.Wilted
            );

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(
                    AudioManager.Instance.witherSound
                );
            }
        }

        LandManager.Instance.OnCropStateChange(
            landID,
            cropState,
            growth,
            health
        );
    }

    void SwitchState(CropState stateToSwitch)
    {
        seed.SetActive(false);
        seedling.SetActive(false);
        harvestable.SetActive(false);
        wilted.SetActive(false);

        switch (stateToSwitch)
        {
            case CropState.Seed:

                seed.SetActive(true);

                break;

            case CropState.Seedling:

                seedling.SetActive(true);

                break;

            case CropState.Harvestable:

                harvestable.SetActive(true);

                break;

            case CropState.Wilted:

                wilted.SetActive(true);

                break;
        }

        cropState =
            stateToSwitch;
    }

    public void RemoveCrop()
    {
        LandManager.Instance.DeregisterCrop(
            landID
        );

        Destroy(gameObject);
    }

    public void Regrow()
    {
        float hoursToRegrow =
            GameTimestamp.DaysToHours(
                seedToGrow.daysToRegrow
            );

        if (useRealTimeGrowth)
        {

            float fraction =
                seedToGrow.daysToGrow > 0f
                    ? 1f -
                      (
                          seedToGrow.daysToRegrow /
                          seedToGrow.daysToGrow
                      )
                    : 0.5f;

            fraction =
                Mathf.Clamp(
                    fraction,
                    0.5f,
                    0.99f
                );

            growth =
                maxGrowSeconds * fraction;

            plantRealTime =
                Time.time - growth;
        }
        else
        {
            growth =
                maxGrowth -
                GameTimestamp.HoursToMinutes(
                    hoursToRegrow
                );
        }

        SwitchState(
            CropState.Seedling
        );
    }
}
