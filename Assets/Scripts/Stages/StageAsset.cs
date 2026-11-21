using UnityEngine;

/// <summary>
/// 1 ステージの定義。どのシーンをロードするか等のメタ情報を持つ。
/// </summary>
[CreateAssetMenu(menuName = "TD/Stage")]
public class StageAsset : ScriptableObject
{
    [Tooltip("一意なステージID 例: stage01")]
    public string stageId;

    [Tooltip("表示名 例: Stage 01")]
    public string displayName;

    [Tooltip("ロードするシーン名（Build Settings に登録された名前と一致させること）")]
    public string sceneName;
}
