using UnityEngine;

/// <summary>
/// タワーの「攻撃する」責務だけを担当するコンポーネント。
/// まずは空の土台として定義し、少しずつ TowerController から攻撃ロジックを移していきます。
/// </summary>
public class TowerAttackController : MonoBehaviour
{
    private float attackInterval;
    private float range;
    private float attackTimer = 0f;
    private GameObject projectilePrefab;
    private Transform firePoint;
    private float projectileTravelTime = 0f;

    public void Configure( float attackInterval, float range, GameObject projectilePrefab, Transform firePoint, float projectileTravelTime)
    {
        this.attackInterval = attackInterval;
        this.range = range;

        SetProjectile(projectilePrefab, firePoint, projectileTravelTime);
    }

    // 球を切り替えられるように別メソッド化
    public void SetProjectile(GameObject projectilePrefab, Transform firePoint, float projectileTravelTime)
    {
        this.projectilePrefab = projectilePrefab;
        this.firePoint = firePoint;
        this.projectileTravelTime = projectileTravelTime;
    }

    private void Update()
    {
        attackTimer += Time.deltaTime;
        if (attackTimer >= attackInterval)
        {
            attackTimer = 0f;
            TryAttack();
        }
    }

    private void TryAttack()
    {
        if (enemiesInRange.Count > 0)
        {
            Attack(enemiesInRange[0]);
        }
    }

    protected virtual void Attack(EnemyController target)
    {
        if (projectilePrefab == null) return;
        if (firePoint == null)
        {
            Debug.LogError($"FirePoint is not assigned on {name}");
            return;
        }

        GameObject bullet = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        Projectile p = bullet.GetComponent<Projectile>();
        p.SetTarget(target.transform, projectileTravelTime);

        PlayAttackAnimation();
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag(Tags.Enemy))
        {
            EnemyController enemy = col.GetComponent<EnemyController>();
            if (!enemiesInRange.Contains(enemy))
            {
                enemiesInRange.Add(enemy);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag(Tags.Enemy))
        {
            EnemyController enemy = col.GetComponent<EnemyController>();
            if (enemiesInRange.Contains(enemy))
            {
                enemiesInRange.Remove(enemy);
            }
        }
    }

}

