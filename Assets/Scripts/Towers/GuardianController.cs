using UnityEngine;
using System.Collections.Generic;

public class GuardianController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float stopDistance = 0.05f;
    [SerializeField] private int hp = 100;
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private float attackInterval = 1.0f;
    private float attackTimer;
    private bool hasMoveTarget;
    private Vector3 moveTarget;
    private bool isDead;
    private GuardianTowerControllerBase ownerTower;
    private List<EnemyController> currentTargets = new List<EnemyController>();

    void Awake()
    {
        ownerTower = GetComponentInParent<GuardianTowerControllerBase>();
    }

    void Update()
    {
        HandleMovement();
        HandleCombat();
        HandleDeath();
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.CompareTag("Enemy"))
        {
            return;
        }

        EnemyController enemy = col.GetComponent<EnemyController>();
        if (enemy != null && !currentTargets.Contains(enemy))
        {
            currentTargets.Add(enemy);
            enemy.EngageGuardian(this);
        }
    }

    public void SetMoveTarget(Vector3 targetPosition)
    {
        moveTarget = targetPosition;
        hasMoveTarget = true;
    }

    private void HandleMovement()
    {
        if (!hasMoveTarget)
        {
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, moveTarget, moveSpeed * Time.deltaTime);
        if (Vector3.Distance(transform.position, moveTarget) <= stopDistance)
        {
            hasMoveTarget = false;
        }
    }

    private void HandleDeath()
    {
        if (isDead || hp > 0)
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
        if (currentTargets.Count == 0)
        {
            return;
        }

        attackTimer += Time.deltaTime;
        EnemyController target = currentTargets[0];
        if (target == null)
        {
            currentTargets.RemoveAt(0);
            return;
        }

        if (attackTimer >= attackInterval)
        {
            attackTimer = 0f;
            target.TakeDamage(attackDamage);
            target.EngageGuardian(this);
            if (target.hp <= 0)
            {
                currentTargets.RemoveAt(0);
            }
        }
    }

    public void TakeDamage(int damage)
    {
        hp -= damage;
    }
}
