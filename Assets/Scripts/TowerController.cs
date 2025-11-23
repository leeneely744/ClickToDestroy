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

    [Tooltip("0 より大きい場合、弾はこの時間でターゲットへ到達できる速度に調整されます。")]
    public float projectileTravelTime = 0f;

    private bool isSelected = false;
    private SpriteRenderer attackRangeRenderer;
    private Money moneyController;
    private TowerPlace towerPlace;

    [SerializeField] public int cost = 70;
    [SerializeField] private int sellRefund = 60;
    [SerializeField] private int upgradeCost = 100;
    private int maxLevel = 3;
    private int currentLevel = 1;

    void Start()
    {
        Transform rangeCircle = transform.Find("AttackRangeCircle");
        if (rangeCircle != null)
        {
            attackRangeRenderer = rangeCircle.GetComponent<SpriteRenderer>();
        }

        if (attackRangeRenderer != null)
        {
            attackRangeRenderer.enabled = false;
        }

        moneyController = FindObjectOfType<Money>();
        if (moneyController == null)
        {
            Debug.LogError("Money controller not found");
        }
    }

    public void OnSelected()
    {
        isSelected = !isSelected;
        if (attackRangeRenderer != null)
        {
            attackRangeRenderer.enabled = isSelected;
        }

        TowerActionPanel.Instance.Show(this);
    }

    public void TurnOffAttackRange()
    {
        isSelected = false;
        if (attackRangeRenderer != null)
        {
            attackRangeRenderer.enabled = false;
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
        p.SetTarget(target.transform, projectileTravelTime);
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

    public void SetTowerPlace(TowerPlace place)
    {
        towerPlace = place;
    }
    
    public void SellTower()
    {
        if (moneyController == null)
        {
            Debug.LogError("Money controller not found");
            return;
        }
        if (towerPlace == null)
        {
            Debug.LogError("TowerPlace not found");
            return;
        }

        moneyController.AddMoney(sellRefund);
        towerPlace.SetOccupied(false);

        Destroy(gameObject);
    }

    public void UpgradeTower()
    {
        Debug.Log("UpgradeTower called");
    }
}
