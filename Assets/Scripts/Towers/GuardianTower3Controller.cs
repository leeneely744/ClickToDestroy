using UnityEngine;

public class GuardianTower3Controller : GuardianTowerControllerBase
{
    protected override int InitialLevelIndex => 2;

    [SerializeField] private GameObject guardianPrefab;
    protected override GameObject GuardianPrefab => guardianPrefab;
}
