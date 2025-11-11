using UnityEngine;
using System.Collections.Generic;

public class TowerController : MonoBehaviour
{
    public float range = 3f;
    public float attackInterval = 1.0f;
    private float attackTimer = 0f;

    private List<EnemyController> enemiesInRange = new List<EnemyController>();

    public GameObject projectilePrefab;
    public Transform firePoint;  // 砲弾の発射位置

    private bool isSelected = false;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void OnSelected()
    {
        isSelected = !isSelected;
        if (spriteRenderer != null)
        {
            spriteRenderer.color = isSelected ? Color.yellow : Color.white;
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
}
