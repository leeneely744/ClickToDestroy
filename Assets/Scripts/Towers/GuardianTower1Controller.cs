using UnityEngine;

public class GuardianTower1Controller : TowerController
{
    [SerializeField] private GameObject nextLevelPrefab;
    [SerializeField] private int maxSoldiers = 3;

    protected override int InitialLevelIndex => 0;
    public override GameObject NextLevelPrefab => nextLevelPrefab;
    public override int GetMaxUnits() => maxSoldiers;
}
