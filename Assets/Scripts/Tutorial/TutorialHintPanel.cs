using System;
using TMPro;
using UnityEngine;

/// <summary>
/// チュートリアルヒントの表示パネル。
/// 表示中は Time.timeScale = 0 でゲームを一時停止し、OK ボタンで再開する。
///
/// Unity エディタでのセットアップ:
///   GameCanvas
///     └── TutorialHintPanel (このスクリプト, Image=半透明背景)
///           ├── MessageText : TextMeshProUGUI → messageText に割当て
///           └── OkButton    : Button (onClick → OnCloseClick)
/// </summary>
public class TutorialHintPanel : MonoBehaviour
{
    public static TutorialHintPanel Instance { get; private set; }

    [SerializeField] private TMP_Text messageText;

    private Action onClosed;
    private float previousTimeScale = 1f;

    private void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);
    }

    public void Show(string message, Action onClosedCallback)
    {
        if (messageText == null)
        {
            Debug.LogError("[TutorialHintPanel] messageText が設定されていません。Inspector を確認してください。", this);
            onClosedCallback?.Invoke();
            return;
        }

        onClosed = onClosedCallback;
        messageText.text = message;

        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        gameObject.SetActive(true);
    }

    // OK ボタンの onClick に割り当てる
    public void OnCloseClick()
    {
        gameObject.SetActive(false);
        Time.timeScale = previousTimeScale;

        var callback = onClosed;
        onClosed = null;
        callback?.Invoke();
    }

    [ContextMenu("Reset Tutorial Hints (全ヒントを未読に戻す)")]
    private void ResetHints()
    {
        TutorialHintService.ResetAll();
        Debug.Log("[TutorialHintPanel] 全ヒントを未読に戻しました。");
    }
}
