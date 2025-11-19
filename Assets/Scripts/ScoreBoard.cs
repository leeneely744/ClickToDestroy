using TMPro;
using UnityEngine;

public class ScoreBoard : MonoBehaviour
{
    public int hp = 100;
    public TextMeshProUGUI hpText;

    public int CurrentHp => hp;

    void Start()
    {
        UpdateHpText();
    }

    public void CalcHp(int damage)
    {
        hp -= damage;
        UpdateHpText();
        if (hp <= 0)
        {
            GameOver();
        }
    }

    public void ResetHp(int value)
    {
        hp = value;
        UpdateHpText();
    }

    private void UpdateHpText()
    {
        if (hpText != null)
        {
            hpText.text = $"HP: {hp}";
        }
    }

    private void GameOver()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.HandleGameOver();
        }
        else
        {
            Debug.Log("Game Over");
        }
    }
}
