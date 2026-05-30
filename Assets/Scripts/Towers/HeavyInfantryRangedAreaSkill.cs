using UnityEngine;

public class HeavyInfantryRangedAreaSkill : GuardianSkill
{
    [SerializeField] private float chance = 0.2f;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float projectileTravelTime = 0.5f;

    public override void OnAttack(EnemyController target, int attackDamage)
    {
        if (Random.value >= chance) return;
        if (projectilePrefab == null || target == null) return;

        var spawnPos = firePoint != null ? firePoint.position : transform.position;
        var go = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        var proj = go.GetComponent<Projectile>();
        if (proj != null)
            proj.SetTarget(target.transform, projectileTravelTime);
    }
}
