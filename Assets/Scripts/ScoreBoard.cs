using TMPro;
using UnityEngine;

public class ScoreBoard : MonoBehaviour
{
    public int hp = 100;
    public TextMeshProUGUI hpText;

    [SerializeField] private GameObject gameOverPanel;

    void Start()
    {
        hpText.text = $"HP: {hp}";
    }

    public void CalcHp(int damage)
    {
        hp -= damage;
        hpText.text = $"HP: {hp}";
        if (hp <= 0)
        {
            GameOver();
        }
    }

    private void GameOver()
    {
        gameOverPanel.SetActive(true);
        Debug.Log("Game Over");
    }
}
