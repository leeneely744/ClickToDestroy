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
    private float? overrideInterval = null;
    private float EffectiveInterval => overrideInterval ?? attackInterval;
    private float range;
    private float attackTimer = 0f;
    private GameObject projectilePrefab;
    private Transform firePoint;
    private float projectileTravelTime = 0f;
    private readonly List<EnemyController> enemiesInRange = new List<EnemyController>();
    private TowerSkill[] skills = new TowerSkill[0];

    protected IReadOnlyList<EnemyController> EnemiesInRange => enemiesInRange;

    [SerializeField] private bool canAttackFlying = false;

    public void Configure(float attackInterval, float range, GameObject projectilePrefab, Transform firePoint, float projectileTravelTime)
    {
        this.attackInterval = attackInterval;
        this.range = range;

        SetProjectile(projectilePrefab, firePoint, projectileTravelTime);

        skills = GetComponents<TowerSkill>();
        foreach (var skill in skills) skill.OnInitialize(this);
    }

    public void SetAttackIntervalOverride(float val) => overrideInterval = val;
    public void ClearAttackIntervalOverride() => overrideInterval = null;

    // StatusPanel に攻撃力を表示するための専用プロパティ。projectile を持たない派生クラスでオーバーライドする
    public virtual int? StatusDamage => null;

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
        if (EffectiveInterval <= 0f)
        {
            return;
        }

        if (attackTimer < EffectiveInterval)
        {
            return;
        }

        if (enemiesInRange.Count <= 0)
        {
            return;
        }

        attackTimer = 0f;
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

        foreach (var skill in skills) skill.OnAttack(target, p != null ? p.damage : 0);

        PlayAttackAnimation();
    }

    /// <summary>
    /// 魔法や弓といったTowerの子オブジェクトがアニメーションを起こす場合と、砲台のようにTower本体がアニメーションを起こす場合がある。
    /// ここでは virtual メソッドとして定義し、必要に応じて派生クラスでオーバーライドできるようにする。
    /// </summary>
    protected virtual void PlayAttackAnimation() { }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag(Tags.Enemy))
        {
            EnemyController enemy = col.GetComponent<EnemyController>();
            if (enemy != null && (!enemy.IsFlying || canAttackFlying) && !enemiesInRange.Contains(enemy))
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
