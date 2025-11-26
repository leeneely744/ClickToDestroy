using UnityEngine;

public class BowTower1Controller : TowerController
{
    [SerializeField] private GameObject nextLevelPrefab;

    public override GameObject NextLevelPrefab => nextLevelPrefab;
}
