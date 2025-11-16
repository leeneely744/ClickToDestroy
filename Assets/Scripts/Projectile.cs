using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 5f;
    public int damage = 10;

    private Transform target;

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
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        // ターゲットへ移動
        Vector3 direction = (target.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;

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
            enemy.TakeDamage(damage);
        }

        Destroy(gameObject);
    }
}
