using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="Items/Item")]
public class ItemData : ScriptableObject
{
    [Header("Info")]
    public string itemName;

    [TextArea]
    public string description;
<<<<<<< Updated upstream
    public Sprite thumbnail;
    public string itemName;
    public GameObject gameModel;

=======

    [Header("Sprites")]
    public Sprite thumbnail;

    // Sprite shown above pot/customer/buffet
    public Sprite itemSprite;

    [Header("3D Model")]
    public GameObject gameModel;

    [Header("Cooking")]
    public bool cookable = true;

    // Result after cooking
    public ItemData cookedVersion;

    // Result after burning
    public ItemData burnedVersion;

    [Header("Servings")]
    public int servings = 10;
>>>>>>> Stashed changes
}

