using UnityEngine;
using UnityEngine.UI;

public class FarmTile : MonoBehaviour
{
    [Header("Settings")]
    public CropData cropData;
    public bool isPlanted = false;
    public bool isReady = false;

    [Header("UI References")]
    public GameObject loadingIcon; // A simple UI Image or Text
    public GameObject readyIcon;   // A simple UI Image or Text

    private float timer = 0f;
    private bool isGrowing = false;

    void Update()
    {
        if (isPlanted && !isReady)
        {
            GrowCrop();
        }
    }

    public void PlantCrop()
    {
        if (isPlanted) return;
        
        // Check if player can afford seed (Simplified: Assume unlocked)
        isPlanted = true;
        isReady = false;
        timer = 0f;
        isGrowing = true;
        
        loadingIcon.SetActive(true);
        readyIcon.SetActive(false);
        Debug.Log("Planted " + cropData.cropName);
    }

    void GrowCrop()
    {
        timer += Time.deltaTime;
        if (timer >= cropData.growthTime)
        {
            isReady = true;
            isGrowing = false;
            loadingIcon.SetActive(false);
            readyIcon.SetActive(true);
            Debug.Log("Crop Ready!");
        }
    }

    public void Harvest()
    {
        if (!isReady) return;

        GameManager.Instance.AddIngredient(cropData.cropName);
        isPlanted = false;
        isReady = false;
        timer = 0f;
        loadingIcon.SetActive(false);
        readyIcon.SetActive(false);
        
        // Reset visual
        transform.localScale = Vector3.one; 
    }
}