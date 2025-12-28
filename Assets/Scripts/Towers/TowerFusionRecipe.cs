using UnityEngine;

/// <summary>
/// タワー同士の合成レシピを定義する ScriptableObject。
/// 例: guardian_lv3 + bow_lv3 -> SamuraiTower
/// </summary>
[CreateAssetMenu(menuName = "Tower/Fusion Recipe", fileName = "TowerFusionRecipe")]
public class TowerFusionRecipe : ScriptableObject
{
    [Header("Ingredients (Tower IDs)")]
    [Tooltip("合成先（ドラッグされる側）のタワー ID。例: guardian_lv3")]
    public string baseTowerId;

    [Tooltip("合成元（ドラッグしてくる側）のタワー ID。例: bow_lv3")]
    public string materialTowerId;

    [Header("Result")]
    [Tooltip("合成後に設置されるタワーのプレハブ。例: SamuraiTower")]
    public GameObject resultTowerPrefab;

    /// <summary>
    /// 2つのタワーIDの組み合わせが、このレシピと一致するかどうかを判定する。
    /// </summary>
    public bool Matches(string towerIdA, string towerIdB)
    {
            return (towerIdA == baseTowerId && towerIdB == materialTowerId) ||
                   (towerIdA == materialTowerId && towerIdB == baseTowerId);
    }
}

