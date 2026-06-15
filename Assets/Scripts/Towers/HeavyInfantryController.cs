using UnityEngine;

public class HeavyInfantryController : GuardianController
{
    [SerializeField] private float areaRadius = 0.52f;
    [SerializeField, Range(0f, 1f)] private float damageRatio = 1f;

    protected override void OnAttackLanded(EnemyController target, int attackDamage)
    {
        int areaDamage = Mathf.RoundToInt(attackDamage * damageRatio);
        var hits = Physics2D.OverlapCircleAll(transform.position, areaRadius);
        foreach (var col in hits)
        {
            var enemy = col.GetComponent<EnemyController>();
            if (enemy == null || enemy.IsDead || enemy.IsFlying || enemy == target)
                continue;
            enemy.TakeDamage(areaDamage, AttackType.Physical);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, areaRadius);
    }
}
