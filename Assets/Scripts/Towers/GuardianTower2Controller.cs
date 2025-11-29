using UnityEngine;

public class GuardianTower2Controller : TowerController
{
    [SerializeField] private GameObject nextLevelPrefab;
    [SerializeField] private int maxSoldiers = 3;

    protected override int InitialLevelIndex => 1;
    public override GameObject NextLevelPrefab => nextLevelPrefab;
    public override int GetMaxUnits() => maxSoldiers;
}
