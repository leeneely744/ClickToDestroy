using UnityEngine;

public class BowTower2Controller : TowerController
{
    [SerializeField] private GameObject nextLevelPrefab;
    public override GameObject NextLevelPrefab => nextLevelPrefab;
    protected override int InitialLevelIndex => 1;
}
