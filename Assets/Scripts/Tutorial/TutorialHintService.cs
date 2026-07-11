using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// チュートリアルヒントの既読管理と表示キュー。
/// - 既読は PlayerPrefs に永続化する（WebGL では IndexedDB に保存される）
/// - 同時に複数のヒントが発火した場合はキューに積んで順番に表示する
/// </summary>
public static class TutorialHintService
{
    private const string PrefsPrefix = "tutorial_hint_";

    private static readonly Queue<TutorialHint> queue = new Queue<TutorialHint>();
    private static bool isShowing;

    /// <summary>未読ならヒントを表示予約する。既読なら何もしない。</summary>
    public static void TryShow(TutorialHint hint)
    {
        if (hint == null)
        {
            Debug.LogError("[TutorialHintService] TryShow: hint が null です。");
            return;
        }

        if (HasSeen(hint.Id))
        {
            return;
        }

        MarkSeen(hint.Id);
        queue.Enqueue(hint);
        ShowNextIfIdle();
    }

    public static bool HasSeen(string id)
    {
        return PlayerPrefs.GetInt(PrefsPrefix + id, 0) == 1;
    }

    /// <summary>
    /// シーンリロード（リトライ）時に呼ぶ。表示中フラグとキューは
    /// static なのでシーンをまたいで残り、リセットしないと以降のヒントが出なくなる。
    /// </summary>
    public static void ClearRuntimeState()
    {
        queue.Clear();
        isShowing = false;
    }

    /// <summary>全ヒントを未読に戻す（動作確認用）。</summary>
    public static void ResetAll()
    {
        foreach (var hint in TutorialHints.All)
        {
            PlayerPrefs.DeleteKey(PrefsPrefix + hint.Id);
        }
        PlayerPrefs.Save();
    }

    private static void MarkSeen(string id)
    {
        PlayerPrefs.SetInt(PrefsPrefix + id, 1);
        PlayerPrefs.Save();
    }

    private static void ShowNextIfIdle()
    {
        if (isShowing || queue.Count == 0)
        {
            return;
        }

        var panel = TutorialHintPanel.Instance;
        if (panel == null)
        {
            Debug.LogWarning("[TutorialHintService] TutorialHintPanel がシーンに存在しません。ヒント表示をスキップします。");
            queue.Clear();
            return;
        }

        isShowing = true;
        var hint = queue.Dequeue();
        panel.Show(hint.Message, OnPanelClosed);
    }

    private static void OnPanelClosed()
    {
        isShowing = false;
        ShowNextIfIdle();
    }
}
