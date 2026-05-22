using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public  struct CropSaveState 
{
    public int landID;

    public string seedToGrow;
    public CropBehaviour.CropState cropState;
    public float growth;
    public float health;

    public CropSaveState(int landID, string seedToGrow, CropBehaviour.CropState cropState, float growth, float health)
    {
        this.landID = landID;
        this.seedToGrow = seedToGrow;
        this.cropState = cropState;
        this.growth = growth;
        this.health = health;
    }
}