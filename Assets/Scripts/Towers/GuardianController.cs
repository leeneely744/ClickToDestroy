using UnityEngine;
using System.Collections.Generic;

public class GuardianController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float stopDistance = 0.05f;
    private int hp = 100;
    public int attack = 10;
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
        HandleDeath();

        // 敵への攻撃処理などはここに追加
        if (currentTargets.Count > 0)
        {
            attackTimer += Time.deltaTime;
            EnemyController target = currentTargets[0];
            if (target != null && !target.IsDead && attackTimer >= attackInterval)
            {
                Debug.Log("攻撃しました!");
                target.TakeDamage(attack);
                if (target.IsDead)
                {
                    Debug.Log("倒しました!");
                    currentTargets.RemoveAt(0);
                }
                attackTimer = 0f;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Enemy"))
        {
            Debug.Log("敵を検出しました!");
            EnemyController enemy = col.GetComponent<EnemyController>();
            if (!currentTargets.Contains(enemy))
            {
                currentTargets.Add(enemy);
            }
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
}
