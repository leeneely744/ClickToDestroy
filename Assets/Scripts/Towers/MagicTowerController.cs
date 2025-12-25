using UnityEngine;

/// <summary>
/// Magic タワー共通のコントローラー。
/// レベルごとの差分は TowerStats と Prefab 側の設定（levelIndex / nextLevelPrefab など）で管理する。
/// </summary>
public class MagicTowerController : TowerController
{
    [SerializeField] private GameObject nextLevelPrefab;

    public override GameObject NextLevelPrefab => nextLevelPrefab;
}
