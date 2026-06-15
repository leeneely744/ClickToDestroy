using UnityEngine;
using Tags = Constants.Tags;

public class HeavyInfantryRangedAreaSkill : GuardianSkill
{
    [SerializeField] private float attackInterval = 2f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float projectileTravelTime = 0.5f;

    private float timer;

    private void Update()
    {
        if (!IsPurchased || Owner.IsInMeleeCombat) return;

        timer += Time.deltaTime;
        if (timer < attackInterval) return;
        timer = 0f;

        var target = FindTarget();
        if (target == null) return;

        ThrowBomb(target);
    }

    private EnemyController FindTarget()
    {
        var hits = Physics2D.OverlapCircleAll(transform.position, attackRange);
        foreach (var hit in hits)
        {
            if (!hit.CompareTag(Tags.Enemy)) continue;
            var enemy = hit.GetComponent<EnemyController>();
            if (enemy != null && !enemy.IsDead && !enemy.IsFlying)
                return enemy;
        }
        return null;
    }

    private void ThrowBomb(EnemyController target)
    {
        if (projectilePrefab == null) return;
        var spawnPos = firePoint != null ? firePoint.position : transform.position;
        var go = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        var proj = go.GetComponent<Projectile>();
        if (proj != null)
            proj.SetTarget(target.transform, projectileTravelTime);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
