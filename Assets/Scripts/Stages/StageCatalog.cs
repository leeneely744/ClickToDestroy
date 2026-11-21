using UnityEngine;

/// <summary>
/// 全ステージの一覧（表示順＝配列順）。ステージセレクト画面がこれを参照してボタンを生成する。
/// </summary>
[CreateAssetMenu(menuName = "TD/Stage Catalog")]
public class StageCatalog : ScriptableObject
{
    [Tooltip("ステージの一覧。配列の順序がそのまま表示順になる。")]
    public StageAsset[] stages;

    /// <summary>
    /// current の次のステージを返す。current が見つからない場合・末尾の場合・stages が未設定の場合は null。
    /// </summary>
    public StageAsset GetNext(StageAsset current)
    {
        if (stages == null)
        {
            return null;
        }

        int index = System.Array.IndexOf(stages, current);
        if (index < 0 || index + 1 >= stages.Length)
        {
            return null;
        }

        return stages[index + 1];
    }
}
