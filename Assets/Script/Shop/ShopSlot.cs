using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class ShopSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image itemIcon;
    public TMP_Text priceText;

    SeedData seedData;

    public void Setup(SeedData seed)
    {
        seedData = seed;

        itemIcon.sprite = seed.thumbnail;
        itemIcon.gameObject.SetActive(true);

        priceText.text = "₱ " + seed.buyPrice;
    }

    public void BuySeed()
    {
        ShopManager.Instance.BuySeed(seedData);
    }

    public void BuyButton()
    {
        ShopManager.Instance.BuySeed(seedData);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        UIManager.Instance.DisplayItemInfo(seedData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UIManager.Instance.DisplayItemInfo(null);
    }
}