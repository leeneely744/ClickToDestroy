using System.Collections.Generic;
using UnityEngine;
using Tags = Constants.Tags;

public class GuardianController : MonoBehaviour, IDefender
{
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float stopDistance = 0.05f;
    [SerializeField] private int maxHp = 100;
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private float attackInterval = 1.0f;
    private float attackTimer;
    private bool hasMoveTarget;
    private Vector3 moveTarget;
    private bool isDead;
    private GuardianTowerControllerBase ownerTower;
    private UnitRangedAttack rangedAttack;
    private int currentHp;
    private List<EnemyController> currentTargets = new List<EnemyController>();
    private Animator animator;
    [SerializeField] private HealthBarController healthBar;

    void Awake()
    {
        ownerTower = GetComponentInParent<GuardianTowerControllerBase>();
        animator = GetComponent<Animator>();
        rangedAttack = GetComponentInChildren<UnitRangedAttack>();

        currentHp = maxHp;

        if (healthBar == null)
        {
            healthBar = GetComponentInChildren<HealthBarController>();
        }

        UpdateHealthBar();
    }

    void Update()
    {
        HandleMovement();
        HandleCombat();
        HandleDeath();
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        // 敵であるか？
        if (!col.CompareTag(Tags.Enemy))
        {
            return;
        }

        // すでに攻撃中か？
        if (rangedAttack != null && rangedAttack.HasTarget)
        {
            return;
        }

        // EnemyController を取得
        EnemyController enemy = col.GetComponent<EnemyController>();
        if (enemy == null)
        {
            return;
        }

        // すでにリストに含まれているか？
        if (!currentTargets.Contains(enemy))
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

    private void HandleMovement()
    {
        bool isWalking = hasMoveTarget;
        if (hasMoveTarget)
        {
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
        if (ownerTower != null)
        {
            ownerTower.OnGuardianDestroyed(ownerTower.AttackInterval);
        }

        Destroy(gameObject);
    }

    private void HandleCombat()
    {
        if (rangedAttack != null && rangedAttack.HasTarget)
        {
            UpdateAttackAnimation(false);
            return;
        }

        if (currentTargets.Count == 0)
        {
            return;
        }

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

        currentHp -= damage;
        UpdateHealthBar();
        if (currentHp <= 0 && animator != null)
        {
            animator.SetTrigger("Die");
        }
    }

    public bool IsDead => currentHp <= 0 || isDead;

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
