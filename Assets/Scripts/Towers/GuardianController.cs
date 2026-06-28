using System.Collections.Generic;
using UnityEngine;
using Tags = Constants.Tags;

public class GuardianController : MonoBehaviour, IDefender, IStatusProvider
{
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float stopDistance = 0.05f;
    [SerializeField] private int maxHp = 100;
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private float attackInterval = 1.0f;
    [SerializeField] private int maxConcurrentTargets = 1;
    private float attackTimer;
    private bool hasMoveTarget;
    private Vector3 moveTarget;
    private bool isDead;
    private bool facingRight = true;
    private GuardianTowerControllerBase ownerTower;
    private UnitRangedAttack rangedAttack;
    private int currentHp;
    private List<EnemyController> currentTargets = new List<EnemyController>();
    private Animator animator;
    private GuardianSkill[] skills;
    [SerializeField] private HealthBarController healthBar;

    void Awake()
    {
        ownerTower = GetComponentInParent<GuardianTowerControllerBase>();
        animator = GetComponent<Animator>();
        rangedAttack = GetComponentInChildren<UnitRangedAttack>();
        skills = GetComponents<GuardianSkill>();

        currentHp = maxHp;

        if (healthBar == null)
        {
            healthBar = GetComponentInChildren<HealthBarController>();
        }

        UpdateHealthBar();
        RegisterClickHandlers();
    }

    private void RegisterClickHandlers()
    {
        foreach (var col in GetComponentsInChildren<Collider2D>())
        {
            if (col.GetComponent<StatusClickHandler>() == null)
                col.gameObject.AddComponent<StatusClickHandler>();
        }
    }

    public StatusInfo GetStatusInfo()
    {
        var sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();
        return new StatusInfo
        {
            displayName = gameObject.name.Replace("(Clone)", "").Trim(),
            icon = sr != null ? sr.sprite : null,
            maxHp = maxHp,
            getCurrentHp = () => currentHp,
            attackDamage = attackDamage > 0 ? attackDamage : (int?)null,
            physicalResistance = 0f,
            magicalResistance = 0f,
        };
    }

    void Update()
    {
        HandleMovement();
        HandleCombat();
        HandleDeath();
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.CompareTag(Tags.Enemy))
        {
            return;
        }

        EnemyController enemy = col.GetComponent<EnemyController>();
        if (enemy == null)
        {
            return;
        }

        // 仕様：近接攻撃は IsFlying=false の敵のみ対象
        if (enemy.IsFlying)
        {
            return;
        }

        if (!currentTargets.Contains(enemy) && currentTargets.Count < maxConcurrentTargets)
        {
            currentTargets.Add(enemy);
            enemy.EngageDefender(this);
        }
    }

    public void SetMoveTarget(Vector3 targetPosition)
    {
        moveTarget = targetPosition;
        hasMoveTarget = true;
    }

    public bool FacingRight => facingRight;

    private void HandleMovement()
    {
        bool isWalking = hasMoveTarget;
        if (hasMoveTarget)
        {
            float dx = moveTarget.x - transform.position.x;
            if (Mathf.Abs(dx) > stopDistance)
                facingRight = dx > 0;

            transform.position = Vector3.MoveTowards(transform.position, moveTarget, moveSpeed * Time.deltaTime);
            if (Vector3.Distance(transform.position, moveTarget) <= stopDistance)
            {
                hasMoveTarget = false;
                isWalking = false;
            }
        }

        if (animator != null)
        {
            animator.SetBool("isWalking", isWalking);
        }
    }

    private void HandleDeath()
    {
        if (isDead || currentHp > 0)
        {
            return;
        }

        isDead = true;
        foreach (var skill in skills) skill.OnDeath();
        if (ownerTower != null)
        {
            ownerTower.OnGuardianDestroyed(ownerTower.AttackInterval);
        }

        Destroy(gameObject);
    }

    private void HandleCombat()
    {
        if (currentTargets.Count == 0)
        {
            // 仕様：近接ターゲットがいないときは遠距離攻撃を有効化
            if (rangedAttack) rangedAttack.enabled = true;
            return;
        }

        // 仕様：近接攻撃と遠距離攻撃では常に近接を優先する
        if (rangedAttack) rangedAttack.enabled = false;

        attackTimer += Time.deltaTime;
        EnemyController target = currentTargets[0];
        if (target == null)
        {
            currentTargets.RemoveAt(0);
            UpdateAttackAnimation(false);
            return;
        }

        UpdateAttackAnimation(true);

        if (attackTimer >= attackInterval)
        {
            attackTimer = 0f;
            target.TakeDamage(attackDamage, AttackType.Physical);
            OnAttackLanded(target, attackDamage);
            foreach (var skill in skills) skill.OnAttack(target, attackDamage);
            target.EngageDefender(this);
            if (target.IsDead)
            {
                currentTargets.RemoveAt(0);
                UpdateAttackAnimation(currentTargets.Count > 0);
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (currentHp <= 0)
        {
            return;
        }

        foreach (var skill in skills)
        {
            if (skill.OnTakeDamage(damage)) return;
        }

        currentHp -= damage;
        UpdateHealthBar();
        if (currentHp <= 0 && animator != null)
        {
            animator.SetTrigger("Die");
        }
    }

    public bool IsDead => currentHp <= 0 || isDead;
    public bool IsInMeleeCombat => currentTargets.Count > 0;

    protected virtual void OnAttackLanded(EnemyController target, int attackDamage) { }

    // SkillCast アニメーションが完成したらここに animator.SetTrigger("SkillCast") を追加する
    public void TriggerSkillAnimation() { }

    private void UpdateAttackAnimation(bool isAttacking)
    {
        if (animator != null)
        {
            animator.SetBool("isAttacking", isAttacking);
            if (isAttacking)
            {
                animator.SetBool("isWalking", false);
            }
        }
    }

    private void UpdateHealthBar()
    {
        if (healthBar == null)
        {
            return;
        }

        float ratio = maxHp > 0 ? (float)currentHp / maxHp : 0f;
        healthBar.SetRatio(ratio);
    }
}
