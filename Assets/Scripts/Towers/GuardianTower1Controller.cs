using UnityEngine;

public class GuardianTower1Controller : TowerController
{
    [SerializeField] private GameObject nextLevelPrefab;

    protected override int InitialLevelIndex => 0;
    public override GameObject NextLevelPrefab => nextLevelPrefab;
}
