using TMPro;
using UnityEngine;

public class Money : MonoBehaviour
{
    public int money = 100;
    public TextMeshProUGUI moneyText;

    void Start()
    {
        moneyText.text = $"Money: {money}";
    }

    public void AddMoney(int amount)
    {
        money += amount;
        moneyText.text = $"Money: {money}";
    }

    public bool SpendMoney(int amount)
    {
        if (money >= amount)
        {
            money -= amount;
            moneyText.text = $"Money: {money}";
            return true;
        }
        return false;
    }
}
