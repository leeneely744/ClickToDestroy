using UnityEngine;

public class CannonTowerController : TowerController
{
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject nextLevelPrefab;

    private CannonTowerStats cannonStats;

    protected override int InitialLevelIndex => 0;
    public override GameObject NextLevelPrefab => nextLevelPrefab;

    protected override void Start()
    {
        base.Start();

        cannonStats = stats as CannonTowerStats;
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

}
