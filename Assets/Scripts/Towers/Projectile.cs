using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float maxLifetime = 10f;
    public float speed = 5f;
    public int damage = 10;
    public AttackType attackType = AttackType.Physical;

    [Header("Hit Effect")]
    [SerializeField] private GameObject hitEffectPrefab;

    [Header("Splash settings (for Cannon, etc.)")]
    [SerializeField] private float splashRadius = 0f; // 0 のときは範囲ダメージなし
    [SerializeField, Range(0f, 1f)]
    private float splashDamageRate = 0.5f; // 本体ダメージの何倍か（割合）
    [SerializeField] private LayerMask enemyLayerMask; // 範囲ダメージを与えたい敵がいるレイヤー

    [SerializeField] private bool rotateToFlight = false;
    [SerializeField] private float rotationOffset = 0f;

    private Transform target;
    private float lifeTimer;

    public void SetTarget(Transform newTarget, float desiredTravelTime = 0f)
    {
        target = newTarget;

        if (target != null && desiredTravelTime > 0f)
        {
            float distance = Vector3.Distance(transform.position, target.position);
            if (distance > 0.01f)
            {
                speed = distance / desiredTravelTime;
            }
        }
    }

    void Update()
    {
        lifeTimer += Time.deltaTime;
        if (lifeTimer >= maxLifetime)
        {
            Destroy(gameObject);
            return;
        }

        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        // ターゲットへ移動
        Vector3 direction = (target.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;

        if (rotateToFlight)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + rotationOffset;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }

        // 一定距離まで近づいたら命中扱い
        if (Vector3.Distance(transform.position, target.position) < 0.2f)
        {
            HitTarget();
        }
    }

    void HitTarget()
    {
        // 敵にダメージを与える
        EnemyController enemy = target.GetComponent<EnemyController>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage, attackType);
        }

        // 範囲ダメージ（Cannon など）を適用
        if (splashRadius > 0f && splashDamageRate > 0f)
        {
            ApplySplashDamage(enemy);
        }

        if (hitEffectPrefab != null)
            Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }

    /// <summary>
    /// メインターゲット周辺の Enemy にも、damage × splashDamageRate のダメージを与える。
    /// Cannon の砲弾など、範囲攻撃を持つ弾プレハブでのみ有効にする想定。
    /// </summary>
    private void ApplySplashDamage(EnemyController mainTarget)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, splashRadius, enemyLayerMask);
        foreach (var hit in hits)
        {
            var enemy = hit.GetComponent<EnemyController>();
            if (enemy == null)
            {
                continue;
            }

            // メインターゲットはすでに通常ダメージを与えているのでスキップ（必要に応じて変更可）
            if (enemy == mainTarget)
            {
                continue;
            }

            int splashDamage = Mathf.RoundToInt(damage * splashDamageRate);
            enemy.TakeDamage(splashDamage, attackType);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (splashRadius > 0f)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, splashRadius);
        }
    }
}
