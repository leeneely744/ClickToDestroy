using UnityEngine;

public class GuardianTower2Controller : GuardianTowerControllerBase
{
    [SerializeField] private GameObject nextLevelPrefab;

    protected override int InitialLevelIndex => 1;
    public override GameObject NextLevelPrefab => nextLevelPrefab;
}
