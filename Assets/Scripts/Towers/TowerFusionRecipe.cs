using UnityEngine;

/// <summary>
/// タワー同士の合成レシピを定義する ScriptableObject。
/// 例: guardian_lv3 + bow_lv3 -> SamuraiTower
/// </summary>
[CreateAssetMenu(menuName = "Tower/Fusion Recipe", fileName = "TowerFusionRecipe")]
public class TowerFusionRecipe : ScriptableObject
{
    [Header("Ingredients (Tower IDs)")]
    [Tooltip("材料Aのタワー ID。例: guardian_lv3")]
    public string towerAId;

    [Tooltip("材料Bのタワー ID。例: bow_lv3")]
    public string towerBId;

    [Header("Result")]
    [Tooltip("合成後に設置されるタワーのプレハブ。例: SamuraiTower")]
    public GameObject resultTowerPrefab;

    /// <summary>
    /// 2つのタワーIDの組み合わせが、このレシピと一致するかどうかを判定する。
    /// </summary>
    public bool Matches(string towerIdA, string towerIdB)
    {
        return (towerIdA == towerAId && towerIdB == towerBId) ||
            (towerIdA == towerBId && towerIdB == towerAId);
    }
}

