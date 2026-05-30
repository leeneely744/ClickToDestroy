using UnityEngine;

public class HeavyInfantryMeleeAreaSkill : GuardianSkill
{
    [SerializeField] private float areaRadius = 0.8f;
    [SerializeField, Range(0f, 1f)] private float damageRatio = 1f;

    public override void OnAttack(EnemyController target, int attackDamage)
    {
        int skillDamage = Mathf.RoundToInt(attackDamage * damageRatio);
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, areaRadius);
        foreach (var col in hits)
        {
            var enemy = col.GetComponent<EnemyController>();
            if (enemy == null || enemy.IsDead || enemy.IsFlying)
                continue;
            if (enemy == target)
                // メインターゲットはすでにGuardianControllerで攻撃しており、
                // ここで再度ダメージを与えると二重攻撃になってしまうため、
                // スキルのダメージは与えない。
                continue;

            enemy.TakeDamage(skillDamage, AttackType.Physical);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, areaRadius);
    }
}
