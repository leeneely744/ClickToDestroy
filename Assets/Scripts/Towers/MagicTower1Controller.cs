using UnityEngine;

public class MagicTower1Controller : TowerController
{
    [SerializeField] private GameObject nextLevelPrefab;

    public override GameObject NextLevelPrefab => nextLevelPrefab;
}
