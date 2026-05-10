using UnityEngine;

public class EnemyAttackController : MonoBehaviour
{
    private AttackStats melee = new AttackStats { damage = 10, attackInterval = 1.5f };
    private AttackStats ranged = new AttackStats();

    [Header("Ranged Attack")]
    [SerializeField] private GameObject rangedProjectilePrefab;
    [Tooltip("null の場合は transform.position（敵の原点）から発射する。ピボットが中心付近であれば見た目上問題ない")]
    [SerializeField] private Transform rangedFirePoint;
    [SerializeField] private LayerMask defenderLayerMask;

    private IDefender engagedDefender;
    private float attackTimer;
    private bool isEngaged;

    private IDefender rangedTarget;
    private float rangedAttackTimer;
    private bool isRangedEngaged;

    private Animator animator;
    private EnemyController enemy;

    public bool IsEngaged => isEngaged || isRangedEngaged;

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
            ClearRangedTarget();
            return;
        }

        // Priority 1: melee
        // 自律検知（meleeAttack.attackRange が設定されている場合）
        if (!isEngaged && melee.damage > 0 && melee.attackRange > 0)
        {
            IDefender found = FindDefenderInRange(melee.attackRange);
            if (found != null) EngageDefender(found);
        }

        if (isEngaged)
        {
            if (engagedDefender == null || engagedDefender.IsDead)
            {
                Disengage();
            }
            else
            {
                attackTimer += Time.deltaTime;
                if (attackTimer >= melee.attackInterval)
                {
                    attackTimer = 0f;
                    engagedDefender.TakeDamage(melee.damage);
                }
                // 近接が有効なら遠距離はスキップ
                return;
            }
        }

        // Priority 2: ranged（近接交戦中でないときのみ）
        if (ranged.damage > 0 && ranged.attackRange > 0 && rangedProjectilePrefab != null)
        {
            UpdateRangedAttack();
        }
    }

    private void UpdateRangedAttack()
    {
        // 死亡したターゲットをクリア
        if (rangedTarget != null && rangedTarget.IsDead)
            ClearRangedTarget();

        // ターゲットがいなければ射程内を探索
        if (rangedTarget == null)
        {
            rangedTarget = FindDefenderInRange(ranged.attackRange);
            isRangedEngaged = rangedTarget != null;
            rangedAttackTimer = 0f;
            UpdateAnimations();
        }

        if (rangedTarget == null) return;

        // ターゲットが射程外に出たかチェック
        if (rangedTarget is not MonoBehaviour targetMono || Vector2.Distance(transform.position, targetMono.transform.position) > ranged.attackRange)
        {
            ClearRangedTarget();
            return;
        }

        rangedAttackTimer += Time.deltaTime;
        if (rangedAttackTimer >= ranged.attackInterval)
        {
            rangedAttackTimer = 0f;
            FireProjectile(rangedTarget);
        }
    }

    private IDefender FindDefenderInRange(float range)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, range, defenderLayerMask);
        foreach (var hit in hits)
        {
            IDefender defender = hit.GetComponent<IDefender>();
            if (defender != null && !defender.IsDead)
                return defender;
        }
        return null;
    }

    private void FireProjectile(IDefender target)
    {
        Vector3 spawnPos = rangedFirePoint != null ? rangedFirePoint.position : transform.position;
        GameObject proj = Instantiate(rangedProjectilePrefab, spawnPos, Quaternion.identity);
        proj.GetComponent<EnemyProjectile>()?.SetTarget(target, ranged.damage);
    }

    private void ClearRangedTarget()
    {
        rangedTarget = null;
        isRangedEngaged = false;
        UpdateAnimations();
    }

    public void EngageDefender(IDefender defender)
    {
        engagedDefender = defender;
        isEngaged = defender != null && !defender.IsDead;
        attackTimer = 0f;
        ClearRangedTarget();
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
        animator.SetBool("isAttacking", IsEngaged);
    }

    public void ApplyData(EnemyData data)
    {
        if (data == null) return;

        melee  = data.meleeAttack;
        ranged = data.rangedAttack;

        if (melee.attackInterval <= 0f)
            melee.attackInterval = 1f;
        if (ranged.damage > 0 && ranged.attackInterval <= 0f)
            ranged.attackInterval = 1f;
    }
}
