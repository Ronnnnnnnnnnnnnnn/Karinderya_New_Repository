using UnityEngine;

[CreateAssetMenu(menuName = "Items/Item")]
public class ItemData : ScriptableObject
{
    [Header("Info")]
    public string itemName;

    [TextArea]
    public string description;

    [Header("Visual")]
    public Sprite thumbnail;

    public Sprite itemSprite;

    public GameObject gameModel;

    public Vector3 handScale = Vector3.one;

    [Header("Type")]
    public bool isIngredient;

    public bool isDish;

    public bool isServing;

    [Header("Cooking")]
    public ItemData cookedDish;

    [Header("Serving")]
    public ItemData servingVersion;
}