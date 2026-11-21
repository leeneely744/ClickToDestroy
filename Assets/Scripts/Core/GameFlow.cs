using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// シーン遷移を集約する静的ハブ。ステージのロード・リトライ・ステージセレクトへの復帰を担う。
/// 「現在のステージ」を static に保持し、シーンロードをまたいで参照できるようにする。
/// すべての遷移は実行直前に Time.timeScale を 1 に戻す（一時停止中からの遷移に対応）。
/// </summary>
public static class GameFlow
{
    public const string StageSelectSceneName = "StageSelect";

    /// <summary>現在プレイ中のステージ。StageSelect を経ずに直接 Play した場合は null。</summary>
    public static StageAsset CurrentStage { get; private set; }

    /// <summary>指定ステージをロードする。</summary>
    public static void LoadStage(StageAsset stage)
    {
        if (stage == null)
        {
            Debug.LogError("[GameFlow] LoadStage: stage が null です。StageAsset を渡してください。");
            return;
        }

        if (string.IsNullOrEmpty(stage.sceneName))
        {
            Debug.LogError($"[GameFlow] LoadStage: StageAsset '{stage.name}' の sceneName が未設定です。Inspector を確認してください。", stage);
            return;
        }

        CurrentStage = stage;
        Time.timeScale = 1f;
        SceneManager.LoadScene(stage.sceneName);
    }

    /// <summary>現在のステージを再ロードする。CurrentStage が未設定ならアクティブシーンを再ロードする。</summary>
    public static void RetryCurrentStage()
    {
        Time.timeScale = 1f;

        if (CurrentStage == null)
        {
            Debug.LogWarning("[GameFlow] RetryCurrentStage: CurrentStage が未設定のため、アクティブシーンを再ロードします。");
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            return;
        }

        SceneManager.LoadScene(CurrentStage.sceneName);
    }

    /// <summary>ステージセレクト画面へ戻る。</summary>
    public static void ReturnToStageSelect()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(StageSelectSceneName);
    }

    // Play 開始時に静的状態をリセットする（Domain Reload 無効化対策）。
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetOnPlay()
    {
        CurrentStage = null;
    }
}
