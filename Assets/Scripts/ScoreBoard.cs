using TMPro;
using UnityEngine;

public class ScoreBoard : MonoBehaviour
{
    public int hp = 100;
    public TextMeshProUGUI hpText;

    void Start()
    {
        hpText.text = $"HP: {hp}";
    }

    public void CalcHp(int damage)
    {
        hp -= damage;
        hpText.text = $"HP: {hp}";
    }
}
