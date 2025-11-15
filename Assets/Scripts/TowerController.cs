using UnityEngine;
using System.Collections.Generic;

public class TowerController : MonoBehaviour
{
    public float attackInterval = 1.0f;
    private float attackTimer = 0f;
    private CircleCollider2D rangeCollider;
    private List<EnemyController> enemiesInRange = new List<EnemyController>();

    public GameObject projectilePrefab;
    public Transform firePoint;  // 砲弾の発射位置

    private bool isSelected = false;
    private SpriteRenderer attackRangeRenderer;

    // For player tracking (optional)
    // private Transform playerTransform;
    // private bool playerInsideRange = false;

    void Start()
    {
        // For player tracking (optional)
        // rangeCollider = GetComponent<CircleCollider2D>();
        
        Transform rangeCircle = transform.Find("AttackRangeCircle");
        if (rangeCircle != null)
        {
            attackRangeRenderer = rangeCircle.GetComponent<SpriteRenderer>();
        }

        if (attackRangeRenderer != null)
        {
            attackRangeRenderer.enabled = false;
        }
    }

    public void OnSelected()
    {
        isSelected = !isSelected;
        if (attackRangeRenderer != null)
        {
            attackRangeRenderer.enabled = isSelected;
        }
    }
    
    void Update()
    {
        attackTimer += Time.deltaTime;

        if (attackTimer >= attackInterval)
        {
            attackTimer = 0f;

            if (enemiesInRange.Count > 0)
            {
                Attack(enemiesInRange[0]);
            }
        }

        // TrackPlayerRange();
    }

    void Attack(EnemyController target)
    {
        if (projectilePrefab == null) return;

        GameObject bullet = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        Projectile p = bullet.GetComponent<Projectile>();
        p.SetTarget(target.transform);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Enemy"))
        {
            EnemyController enemy = col.GetComponent<EnemyController>();
            if (!enemiesInRange.Contains(enemy))
            {
                enemiesInRange.Add(enemy);
            }
        }
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Enemy"))
        {
            EnemyController enemy = col.GetComponent<EnemyController>();
            if (enemiesInRange.Contains(enemy))
            {
                enemiesInRange.Remove(enemy);
            }
        }
    }

    // private void TrackPlayerRange()
    // {
    //     if (playerTransform == null)
    //     {
    //         GameObject playerObj = GameObject.Find("Player");
    //         if (playerObj != null)
    //         {
    //             playerTransform = playerObj.transform;
    //         }
    //     }

    //     if (playerTransform == null || rangeCollider == null)
    //     {
    //         return;
    //     }

    //     float worldRadius = rangeCollider.radius * transform.lossyScale.x;
    //     float distance = Vector2.Distance(transform.position, playerTransform.position);
    //     bool isInside = distance <= worldRadius;

    //     if (isInside && !playerInsideRange)
    //     {
    //         Debug.Log($"Player entered range (distance {distance:F2}, radius {worldRadius:F2})");
    //     }

    //     playerInsideRange = isInside;
    // }
}
