using UnityEngine;
using System.Collections.Generic;

public class TowerController : MonoBehaviour
{
    public float range = 3f;
    public float attackInterval = 1.0f;
    private float attackTimer = 0f;

    private List<EnemyController> enemiesInRange = new List<EnemyController>();

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
        Debug.Log($"Tower attacking enemy: {target.gameObject.name}");
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
