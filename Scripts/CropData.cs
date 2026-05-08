using UnityEngine;

[CreateAssetMenu(fileName = "NewCrop", menuName = "Karinderya/Crop Data")]
public class CropData : ScriptableObject
{
    public string cropName;
    public float growthTime = 5f; // Seconds
    public int unlockCost = 50;
    public int sellPrice = 20;
    public Sprite icon; // Assign in Inspector
}