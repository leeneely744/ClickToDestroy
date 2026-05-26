using UnityEngine;

public class PaladinMagicAreaSkill : GuardianSkill
{
    [SerializeField] private float chance = 0.15f;
    [SerializeField] private float areaRadius = 1.5f;
    [SerializeField] private float damageRatio = 0.8f;
    [SerializeField] private GameObject effectPrefab;
    [SerializeField] private float effectDuration = 1f;
    [SerializeField] private float effectOffset = 1f;
    [SerializeField] private int effectSortingOrder = 5;

    public override void OnAttack(EnemyController target, int attackDamage)
    {
        if (Random.value >= chance) return;

        Owner.TriggerSkillAnimation();
        SpawnEffect();

        int skillDamage = Mathf.RoundToInt(attackDamage * damageRatio);
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, areaRadius);
        foreach (var col in hits)
        {
            var enemy = col.GetComponent<EnemyController>();
            if (enemy != null && !enemy.IsDead)
                enemy.TakeDamage(skillDamage, AttackType.Magic);
        }
    }

    private void SpawnEffect()
    {
        if (effectPrefab == null) return;

        float xOffset = Owner.FacingRight ? effectOffset : -effectOffset;
        Vector3 pos = transform.position + new Vector3(xOffset, 0f, 0f);
        GameObject effect = Instantiate(effectPrefab, pos, Quaternion.identity);
        Destroy(effect, effectDuration);

        foreach (var sr in effect.GetComponentsInChildren<SpriteRenderer>())
            sr.sortingOrder = effectSortingOrder;
    }
}
