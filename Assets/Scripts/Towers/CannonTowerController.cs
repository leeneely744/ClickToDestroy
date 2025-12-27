using UnityEngine;

public class CannonTowerController : TowerController
{
    [SerializeField] private GameObject nextLevelPrefab;

    protected override int InitialLevelIndex => 0;
    public override GameObject NextLevelPrefab => nextLevelPrefab;
}
