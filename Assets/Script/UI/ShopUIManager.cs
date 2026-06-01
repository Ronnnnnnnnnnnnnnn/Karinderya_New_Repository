using TMPro;
using UnityEngine;

public class ShopUIManager : MonoBehaviour
{
    public static ShopUIManager Instance;

    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemDescText;

    void Awake()
    {
        Instance = this;
    }

    public void DisplayShopItem(ItemData item)
    {
        if(item == null) return;

        itemNameText.text = item.name;
        itemDescText.text = item.description;
    }

    public void ClearShopItem()
    {
        itemNameText.text = "";
        itemDescText.text = "";
    }
}