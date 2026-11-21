using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [Header("Core References")]
    [SerializeField] private ScoreBoard scoreBoard;
    [SerializeField] private Money moneyUi;
    [SerializeField] private Waves wavesUi;

    [Header("Result Panels")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject gameClearPanel;

    private int initialHp;
    private int initialMoney;
    private bool isGameActive;

    protected override void OnSingletonAwake()
    {
        CacheInitialValues();
    }

    private void Start()
    {
        InitializeGame();

        // ゲームの目的と基本操作のヒント（初回のみ表示）
        TutorialHintService.TryShow(TutorialHints.Intro);
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

    public void UpdateWave(int current, int total)
    {
        if (wavesUi != null)
        {
            wavesUi.SetWave(current, total);
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

        if (wavesUi != null)
        {
            wavesUi.ResetWave();
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
        GameFlow.RetryCurrentStage();
    }

    public void ReturnToStageSelect()
    {
        GameFlow.ReturnToStageSelect();
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
