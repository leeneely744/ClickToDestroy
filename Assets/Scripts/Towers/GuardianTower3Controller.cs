using UnityEngine;

public class GuardianTower3Controller : TowerController
{
    [SerializeField] private int maxSoldiers = 4;

    protected override int InitialLevelIndex => 2;
    public override int GetMaxUnits() => maxSoldiers;
}
