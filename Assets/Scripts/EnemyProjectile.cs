using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [Tooltip("移動速度（Unity units/秒）")]
    [SerializeField] private float speed = 8f;
    [Tooltip("この秒数を超えると自動消滅する（秒）")]
    [SerializeField] private float maxLifetime = 10f;
    [SerializeField] private bool rotateToFlight = false;
    [SerializeField] private float rotationOffset = 0f;

    private IDefender target;
    private int damage;
    private float lifeTimer;

    public void SetTarget(IDefender target, int damage)
    {
        this.target = target;
        this.damage = damage;
    }

    private void Update()
    {
        lifeTimer += Time.deltaTime;
        if (lifeTimer >= maxLifetime)
        {
            Destroy(gameObject);
            return;
        }

        if (target == null || target.IsDead)
        {
            Destroy(gameObject);
            return;
        }

        if (target is not MonoBehaviour targetMono)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 direction = (targetMono.transform.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;

        if (rotateToFlight)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + rotationOffset;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }

        if (Vector3.Distance(transform.position, targetMono.transform.position) < 0.2f)
        {
            target.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
