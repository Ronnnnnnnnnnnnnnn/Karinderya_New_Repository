using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="Items/Item")]

public class ItemData : ScriptableObject
{
    public string description;

    public Sprite thumbnail;

    public GameObject gameModel;
    
    [Header("Hand Settings")]
    public Vector3 handScale = Vector3.one;
}
