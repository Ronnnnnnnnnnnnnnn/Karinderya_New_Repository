using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="Items/Seed")]
public class SeedData : ItemData
{
    public float daysToGrow;

    public ItemData cropToYield;

    public GameObject seedling;

    [Header("Regrowable")]

    public bool regrowable;

    public float daysToRegrow;
}
