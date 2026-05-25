using UnityEngine;

public class PaladinMagicAreaSkill : GuardianSkill
{
    [SerializeField] private float chance = 0.15f;
    [SerializeField] private float areaRadius = 1.5f;
    [SerializeField] private float damageRatio = 0.8f;

    public override void OnAttack(EnemyController target, int attackDamage)
    {
        if (Random.value >= chance) return;

        int skillDamage = Mathf.RoundToInt(attackDamage * damageRatio);
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, areaRadius);
        foreach (var col in hits)
        {
            var enemy = col.GetComponent<EnemyController>();
            if (enemy != null && !enemy.IsDead)
                enemy.TakeDamage(skillDamage, AttackType.Magic);
        }
    }
}
