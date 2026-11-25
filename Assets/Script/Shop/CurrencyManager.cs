using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance;

    public int coins = 100;

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        if (SaveManager.HasSave)
            coins = SaveManager.Data.coins;
    }

    void Start()
    {
        Debug.Log(
            "[MONEY] Starting Coins: " +
            coins
        );

        RefreshCoinUI();
    }

    public bool SpendCoins(int amount)
    {
        Debug.Log(
            "[MONEY] Attempt Spend: " +
            amount
        );

        if(coins >= amount)
        {
            coins -= amount;

            Debug.Log(
                "[MONEY] Coins Left: " +
                coins
            );

            RefreshCoinUI();

            return true;
        }

        Debug.Log(
            "[MONEY] Not enough coins"
        );

        return false;
    }

    public void AddCoins(int amount)
    {
        coins += amount;

        if (coins < 0)
            coins = 0;

        Debug.Log("[MONEY] Added Coins: " + amount);
        Debug.Log("[MONEY] Total Coins: " + coins);

        RefreshCoinUI();
    }

    void RefreshCoinUI()
    {
        if (UIManager.Instance != null)
            UIManager.Instance.UpdateCoinUI(coins);
    }
}
