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
    }

    void Start()
    {
        Debug.Log(
            "[MONEY] Starting Coins: " +
            coins
        );

        UIManager.Instance.UpdateCoinUI(coins);
    }

    // =====================================================
    // SPEND
    // =====================================================

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

            UIManager.Instance.UpdateCoinUI(coins);

            return true;
        }

        Debug.Log(
            "[MONEY] Not enough coins"
        );

        return false;
    }

    // =====================================================
    // ADD
    // =====================================================

    public void AddCoins(int amount)
    {
        coins += amount;

        Debug.Log(
            "[MONEY] Added Coins: " +
            amount
        );

        Debug.Log(
            "[MONEY] Total Coins: " +
            coins
        );

        UIManager.Instance.UpdateCoinUI(coins);
    }
}