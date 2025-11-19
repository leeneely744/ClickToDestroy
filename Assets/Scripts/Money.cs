using TMPro;
using UnityEngine;

public class Money : MonoBehaviour
{
    public int money = 100;
    public TextMeshProUGUI moneyText;

    public int CurrentMoney => money;

    void Start()
    {
        UpdateMoneyText();
    }

    public void AddMoney(int amount)
    {
        money += amount;
        UpdateMoneyText();
    }

    public bool SpendMoney(int amount)
    {
        if (money >= amount)
        {
            money -= amount;
            UpdateMoneyText();
            return true;
        }
        return false;
    }

    public void ResetMoney(int value)
    {
        money = value;
        UpdateMoneyText();
    }

    private void UpdateMoneyText()
    {
        if (moneyText != null)
        {
            moneyText.text = $"Money: {money}";
        }
    }
}
