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

    public bool SpendCoins(int amount)
    {
        if(coins >= amount)
        {
            coins -= amount;
            UIManager.Instance.UpdateCoinUI(coins);
            return true;
        }

        return false;
    }

    public void AddCoins(int amount)
    {
        coins += amount;
        UIManager.Instance.UpdateCoinUI(coins);
    }
}