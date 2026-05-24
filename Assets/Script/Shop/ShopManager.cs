using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    public SeedData[] seedsForSale;

    [Header("UI")]
    public Transform shopContainer;
    public GameObject shopSlotPrefab;

    void Start()
    {
        GenerateShop();
    }

    void GenerateShop()
    {
        foreach(SeedData seed in seedsForSale)
        {
            GameObject slotObj =
                Instantiate(shopSlotPrefab, shopContainer);

            ShopSlot slot =
                slotObj.GetComponent<ShopSlot>();

            slot.Setup(seed);
        }
    }

    public void BuySeed(SeedData seed)
    {
        if(CurrencyManager.Instance.SpendCoins(seed.buyPrice))
        {
            InventoryManager.Instance.AddItem(seed);
            UIManager.Instance.RenderInventory();
        }
        else
        {
            Debug.Log("Not enough coins");
        }
    }
}