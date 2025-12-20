using UnityEngine;

public class EnemyAttackController : MonoBehaviour
{
    private float attackInterval = 1.5f;
    private int defenderDamage = 10;

    private IDefender engagedDefender;
    private float attackTimer;
    private bool isEngaged;

    private Animator animator;
    private EnemyController enemy; // 死亡判定などに使用

    public bool IsEngaged => isEngaged;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        enemy = GetComponent<EnemyController>();
        UpdateAnimations();
    }

    private void Update()
    {
        // 敵自身が死んでいるなら何もしない
        if (enemy != null && enemy.IsDead)
        {
            if (isEngaged) Disengage();
            return;
        }

        // 交戦していない or 相手がいない/死んでいるなら解除
        if (!isEngaged || engagedDefender == null || engagedDefender.IsDead)
        {
            if (isEngaged)
            {
                Disengage();
            }
            return;
        }

        attackTimer += Time.deltaTime;
        if (attackTimer >= attackInterval)
        {
            attackTimer = 0f;
            engagedDefender.TakeDamage(defenderDamage);
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
        if (data == null)
        {
            return;
        }

        defenderDamage = data.attackPower;
        attackInterval = data.attackInterval;
    }
}
