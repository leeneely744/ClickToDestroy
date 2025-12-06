using UnityEngine;

public class CannonTowerController : TowerController
{
    [SerializeField] private GameObject nextLevelPrefab;

    protected override int InitialLevelIndex => 0;
    public override GameObject NextLevelPrefab => nextLevelPrefab;

    public override bool UpgradeTower()
    {
        var visualLevelData = GetCannonTowerStatsData(levelIndex);
        if (visualLevelData != null)
        {
            // visualPrefab を更新

            var animator = GetComponent<Animator>();
            if (animator != null)
            {
                animator.runtimeAnimatorController = visualLevelData.animatorController;
            }
        }
        return base.UpgradeTower();
    }

    private CannonLevelData GetCannonTowerStatsData(int levelIndex)
    {
        // TowerController.cs で protected にした `TowerStats stats` には visuals はない。
        // そのため、CannonTowerStats にキャストしてアクセスする必要がある。
        var cannonStats = Stats as CannonTowerStats;
        if (cannonStats == null)
        {
            Debug.LogWarning($"CannonTowerStats not assigned to {name}.");
            return null;
        }

        if (cannonStats.visuals == null)
        {
            Debug.LogWarning($"Tower stats visuals not set for {name}.");
            return null;
        }

        if (levelIndex < 0 || levelIndex >= cannonStats.visuals.Length)
        {
            return null;
        }

        return cannonStats.visuals[levelIndex];
    }
}
