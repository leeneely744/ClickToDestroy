using System.Collections.Generic;
using UnityEngine;
using Tags = Constants.Tags;

/// <summary>
/// ユニット用の遠距離攻撃コンポーネント。
/// AttackRangeCircle の子オブジェクトに付けて、トリガー範囲で敵を検知して攻撃する。
/// </summary>
public class UnitRangedAttack : MonoBehaviour
{
    [Header("Attack")]
    [SerializeField] private float attackInterval = 1.0f;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float projectileTravelTime = 0f;
    [SerializeField] private string shotTriggerName = "Shot";
    [SerializeField] private bool canTargetFlying = false;

    private float attackTimer;
    private readonly List<EnemyController> enemiesInRange = new List<EnemyController>();
    private Animator animator;

    public bool HasTarget => GetCurrentTarget() != null;

    private void Update()
    {
        if (attackInterval <= 0f)
        {
            return;
        }

        attackTimer += Time.deltaTime;
        if (attackTimer < attackInterval)
        {
            return;
        }

        var target = GetCurrentTarget();
        if (target == null)
        {
            return;
        }

        attackTimer = 0f;
        Attack(target);
    }

    private void Awake()
    {
        animator = GetComponentInParent<Animator>();
    }

    private void Attack(EnemyController target)
    {
        if (projectilePrefab == null)
        {
            return;
        }

        var spawnPoint = firePoint != null ? firePoint.position : transform.position;
        GameObject bullet = Instantiate(projectilePrefab, spawnPoint, Quaternion.identity);
        Projectile projectile = bullet.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.SetTarget(target.transform, projectileTravelTime);
        }

        if (animator != null && !string.IsNullOrEmpty(shotTriggerName))
        {
            animator.SetTrigger(shotTriggerName);
        }
    }

    public EnemyController GetCurrentTarget()
    {
        for (int i = enemiesInRange.Count - 1; i >= 0; i--)
        {
            var enemy = enemiesInRange[i];
            if (enemy == null || enemy.IsDead)
            {
                enemiesInRange.RemoveAt(i);
                continue;
            }

            return enemy;
        }

        return null;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.CompareTag(Tags.Enemy))
        {
            return;
        }

        var enemy = col.GetComponent<EnemyController>();
        if (enemy == null)
        {
            return;
        }

        if (!enemy.IsFlying || canTargetFlying)
        {
            if (!enemiesInRange.Contains(enemy))
                enemiesInRange.Add(enemy);
        }
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (!col.CompareTag(Tags.Enemy))
        {
            return;
        }

        var enemy = col.GetComponent<EnemyController>();
        if (enemy == null)
        {
            return;
        }

        enemiesInRange.Remove(enemy);
    }
}
