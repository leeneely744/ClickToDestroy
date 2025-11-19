using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Core References")]
    [SerializeField] private ScoreBoard scoreBoard;
    [SerializeField] private Money moneyUi;

    [Header("Result Panels")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject gameClearPanel;

    private int initialHp;
    private int initialMoney;
    private bool isGameActive;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        CacheInitialValues();
    }

    private void Start()
    {
        InitializeGame();
    }

    private void CacheInitialValues()
    {
        if (scoreBoard != null)
        {
            initialHp = scoreBoard.CurrentHp;
        }

        if (moneyUi != null)
        {
            initialMoney = moneyUi.CurrentMoney;
        }
    }

    public void InitializeGame()
    {
        isGameActive = true;
        Time.timeScale = 1f;

        if (scoreBoard != null)
        {
            scoreBoard.ResetHp(initialHp);
        }

        if (moneyUi != null)
        {
            moneyUi.ResetMoney(initialMoney);
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (gameClearPanel != null)
        {
            gameClearPanel.SetActive(false);
        }
    }

    public void HandleGameOver()
    {
        if (!isGameActive)
        {
            return;
        }

        isGameActive = false;
        Time.timeScale = 0f;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }

    public void HandleGameClear()
    {
        if (!isGameActive)
        {
            return;
        }

        isGameActive = false;
        Time.timeScale = 0f;

        if (gameClearPanel != null)
        {
            gameClearPanel.SetActive(true);
        }
    }

    public void RetryGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ExitGame()
    {
        Time.timeScale = 1f;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
