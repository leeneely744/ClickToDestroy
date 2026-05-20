using UnityEngine;

/// <summary>
/// Magic タワー共通のコントローラー。
/// レベルごとの差分は TowerStats と Prefab 側の設定（levelIndex / nextLevelPrefab など）で管理する。
/// </summary>
public class MagicTowerController : TowerController
{
    [SerializeField] private int initialLevelIndex = 0;
    [SerializeField] private GameObject nextLevelPrefab;

    protected override int InitialLevelIndex => initialLevelIndex;
    public override GameObject NextLevelPrefab => nextLevelPrefab;
}
