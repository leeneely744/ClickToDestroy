using UnityEngine;

public class MagicTower2Controller : TowerController
{
    [SerializeField] private GameObject nextLevelPrefab;

    protected override int InitialLevelIndex => 1;
    public override GameObject NextLevelPrefab => nextLevelPrefab;
}
