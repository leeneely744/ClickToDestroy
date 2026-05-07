using UnityEngine;

public class EnemyAttackController : MonoBehaviour
{
    private AttackStats melee = new AttackStats { damage = 10, attackInterval = 1.5f };
    private AttackStats ranged = new AttackStats();

    private IDefender engagedDefender;
    private float attackTimer;
    private bool isEngaged;

    private Animator animator;
    private EnemyController enemy;

    public bool IsEngaged => isEngaged;

    public AttackStats Melee  => melee;
    public AttackStats Ranged => ranged;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        enemy = GetComponent<EnemyController>();
        UpdateAnimations();
    }

    private void Update()
    {
        if (enemy != null && enemy.IsDead)
        {
            if (isEngaged) Disengage();
            return;
        }

        if (!isEngaged || engagedDefender == null || engagedDefender.IsDead)
        {
            if (isEngaged) Disengage();
            return;
        }

        attackTimer += Time.deltaTime;
        if (attackTimer >= melee.attackInterval)
        {
            attackTimer = 0f;
            engagedDefender.TakeDamage(melee.damage);
        }
    }

    public void EngageDefender(IDefender defender)
    {
        engagedDefender = defender;
        isEngaged = defender != null && !defender.IsDead;
        attackTimer = 0f;
        UpdateAnimations();
    }

    public void Disengage()
    {
        engagedDefender = null;
        isEngaged = false;
        attackTimer = 0f;
        UpdateAnimations();
    }

    private void UpdateAnimations()
    {
        if (animator == null) return;
        animator.SetBool("isAttacking", isEngaged);
    }

    public void ApplyData(EnemyData data)
    {
        if (data == null) return;

        melee  = data.meleeAttack;
        ranged = data.rangedAttack;

        if (melee.attackInterval <= 0f)
            melee.attackInterval = 1f;
    }
}
