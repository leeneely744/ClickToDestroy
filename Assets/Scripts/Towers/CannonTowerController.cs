using UnityEngine;

public class CannonTowerController : TowerController
{
    [SerializeField] private GameObject nextLevelPrefab;

    [SerializeField] private string attackTriggerName = "isAttack";
    private int attackTriggerHash;

    protected override int InitialLevelIndex => 0;
    public override GameObject NextLevelPrefab => nextLevelPrefab;

    protected override void Start()
    {
        base.Start();

        attackTriggerHash = Animator.StringToHash(attackTriggerName);
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    protected override void Attack(EnemyController target)
    {
        base.Attack(target);
        PlayAttackAnimation();
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
