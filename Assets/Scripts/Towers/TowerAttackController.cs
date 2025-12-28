using System.Collections.Generic;
using UnityEngine;
using Tags = Constants.Tags;

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
    private readonly List<EnemyController> enemiesInRange = new List<EnemyController>();

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
        TryAttack();
    }

    private void TryAttack()
    {
        if (attackInterval <= 0f)
        {
            return;
        }

        if (attackTimer < attackInterval)
        {
            return;
        }

        if (enemiesInRange.Count <= 0)
        {
            return;
        }

        attackTimer = 0f;
        Debug.Log($"TowerAttackController: Attack enemy {enemiesInRange[0].name}");
        Attack(enemiesInRange[0]);
    }

    protected virtual void Attack(EnemyController target)
    {
        if (projectilePrefab == null)
        {
            return;
        }
        if (firePoint == null)
        {
            Debug.LogError($"FirePoint is not assigned on {name}");
            return;
        }

        GameObject bullet = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        Projectile p = bullet.GetComponent<Projectile>();
        if (p != null && target != null)
        {
            p.SetTarget(target.transform, projectileTravelTime);
        }

        PlayAttackAnimation();
    }

    /// <summary>
    /// 魔法や弓といったTowerの子オブジェクトがアニメーションを起こす場合と、砲台のようにTower本体がアニメーションを起こす場合がある。
    /// ここでは virtual メソッドとして定義し、必要に応じて派生クラスでオーバーライドできるようにする。
    /// </summary>
    protected virtual void PlayAttackAnimation()
    {
        Debug.LogWarning($"PlayAttackAnimation not implemented in {name}");
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag(Tags.Enemy))
        {
            EnemyController enemy = col.GetComponent<EnemyController>();
            if (enemy != null && !enemiesInRange.Contains(enemy))
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
            if (enemy != null && enemiesInRange.Contains(enemy))
            {
                enemiesInRange.Remove(enemy);
            }
        }
    }

}
