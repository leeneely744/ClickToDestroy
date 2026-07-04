using System;

/// <summary>
/// 「移動先クリック待ち」モードをゲーム全体で同時に 1 つに限定するコーディネーター。
/// Hero とガーディアンタワーがそれぞれ Update でマウスをポーリングしているため、
/// 両方の移動モードが同時に有効になると 1 クリックを両者が消費してしまう。
/// このクラスで排他制御し、新しいモードの開始時に既存のモードをキャンセルする。
/// </summary>
public static class MoveModeCoordinator
{
    private static object activeOwner;
    private static Action cancelActive;

    /// <summary>
    /// 移動モードを開始する。別のオーナーがアクティブな場合はそちらを先にキャンセルする。
    /// </summary>
    public static void Activate(object owner, Action onCancel)
    {
        if (activeOwner != null && activeOwner != owner)
        {
            var cancel = cancelActive;
            activeOwner = null;
            cancelActive = null;
            cancel?.Invoke();
        }

        activeOwner = owner;
        cancelActive = onCancel;
    }

    /// <summary>
    /// owner が現在アクティブな場合のみ移動モードを終了する。
    /// （他のオーナーに切り替わった後に呼ばれても影響しない）
    /// </summary>
    public static void Deactivate(object owner)
    {
        if (activeOwner != owner)
        {
            return;
        }

        activeOwner = null;
        cancelActive = null;
    }

    /// <summary>
    /// 状態を完全にクリアする。シーンリロード（リトライ）時に呼ぶこと。
    /// static フィールドはシーンをまたいで生存するため、破棄済みオブジェクトへの参照を残さない。
    /// </summary>
    public static void Clear()
    {
        activeOwner = null;
        cancelActive = null;
    }
}
