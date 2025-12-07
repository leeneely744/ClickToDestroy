using UnityEngine;

public class CannonTowerController : TowerController
{
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject nextLevelPrefab;

    private string attackTriggerName = "isAttack";
    private CannonTowerStats cannonStats;
    private int attackTriggerHash;

    protected override int InitialLevelIndex => 0;
    public override GameObject NextLevelPrefab => nextLevelPrefab;

    protected override void Start()
    {
        base.Start();

        cannonStats = stats as CannonTowerStats;
        attackTriggerHash = Animator.StringToHash(attackTriggerName);
        ApplyAnimatorForLevel(levelIndex);
    }

    protected override void Update()
    {
        base.Update();

        // Safety: re-assign if Animator was missing a controller at play start
        if (animator != null && animator.runtimeAnimatorController == null)
        {
            ApplyAnimatorForLevel(levelIndex);
        }
    }

    protected override void Attack(EnemyController target)
    {
        base.Attack(target);
        PlayAttackAnimation();
    }

    public override bool UpgradeTower()
    {
        if (cannonStats == null)
        {
            Debug.LogError("[CannonTower] cannonStats is null; cannot upgrade.");
            return false;
        }

        int nextLevelIndex = levelIndex + 1;
        if (cannonStats.levels == null || nextLevelIndex >= cannonStats.levels.Length)
        {
            Debug.LogWarning($"[CannonTower] Already at max level ({levelIndex}).");
            return false;
        }

        int upgradeCost = GetUpgradeCost();
        if (moneyController != null && upgradeCost > 0)
        {
            if (!moneyController.SpendMoney(upgradeCost))
            {
                Debug.LogWarning($"[CannonTower] Not enough money to upgrade. cost={upgradeCost}, money={moneyController.CurrentMoney}");
                return false;
            }
        }

        ApplyStats(nextLevelIndex);
        ApplyAnimatorForLevel(nextLevelIndex);
        Debug.Log($"[CannonTower] Upgraded to level {nextLevelIndex + 1} on same prefab {name}");
        return true;
    }

    public override int GetUpgradeCost()
    {
        if (cannonStats == null || cannonStats.levels == null)
        {
            return 0;
        }

        int nextLevelIndex = levelIndex + 1;
        if (nextLevelIndex < 0 || nextLevelIndex >= cannonStats.levels.Length)
        {
            return 0;
        }

        var data = cannonStats.levels[nextLevelIndex];
        return data != null ? data.cost : 0;
    }

    private void ApplyAnimatorForLevel(int index)
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (animator == null || cannonStats == null)
        {
            return;
        }

        if (cannonStats.HasAnimatorForLevel(index))
        {
            animator.runtimeAnimatorController = cannonStats.GetAnimatorForLevel(index);
        }
    }

    private void PlayAttackAnimation()
    {
        if (animator == null)
        {
            return;
        }

        animator.SetTrigger(attackTriggerHash);
    }

}
