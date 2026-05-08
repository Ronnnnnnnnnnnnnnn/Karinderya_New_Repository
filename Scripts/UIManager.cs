using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public Text moneyText;
    public Text inventoryText;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void UpdateMoneyUI(float amount)
    {
        moneyText.text = "Money: ₱" + amount.ToString("F0");
    }

    public void UpdateInventoryUI()
    {
        string inv = "Inventory: ";
        foreach (var item in GameManager.Instance.ingredients)
        {
            inv += item + " ";
        }
        inventoryText.text = inv;
    }
}