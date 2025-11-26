using UnityEngine;
using System.Collections.Generic;

public class TowerController : MonoBehaviour
{
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
    private int levelIndex = 0;

    /// <summary>
    /// TowerStats
    /// </summary>
    [SerializeField] private TowerStats stats;
    private int maxLevelIndex;
    private string towerName;
    [HideInInspector] public int cost;
    private int sellRefund;
    private float attackDamage;
    private float attackInterval;
    private float range;

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

        levelIndex = InitialLevelIndex;
        ApplyStatus();
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

        var archerAnimator = GetComponentInChildren<ArcherAnimatorController>();
        if (archerAnimator != null)
        {
            archerAnimator.PlayAttack();
        }
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

        moneyController.AddMoney(GetSellValue());
        towerPlace.SetOccupied(false);

        Destroy(gameObject);
    }

    public bool UpgradeTower()
    {
        var nextPrefab = NextLevelPrefab;
        if (nextPrefab == null || towerPlace == null)
        {
            return false;
        }

        int upgradeCost = GetUpgradeCost();
        Debug.Log($"[TowerController] Try upgrade {name} cost={upgradeCost}");
        if (moneyController != null && upgradeCost > 0)
        {
            if (!moneyController.SpendMoney(upgradeCost))
            {
                Debug.Log("Not enough money to upgrade tower.");
                return false;
            }
        }

        var newTowerObj = Instantiate(nextPrefab, transform.position, Quaternion.identity);
        var newController = newTowerObj.GetComponent<TowerController>();
        if (newController != null)
        {
            newController.SetTowerPlace(towerPlace);
        }

        Destroy(gameObject);
        return true;
    }

    private void ApplyStatus()
    {
        var data = GetLevelData(levelIndex);
        if (data == null)
        {
            Debug.LogError($"Tower level data not found for index {levelIndex}");
            return;
        }
        towerName = data.towerName;
        cost = data.cost;
        sellRefund = data.sellRefund;
        attackDamage = data.attackDamage;
        attackInterval = data.attackInterval;
        range = data.range;
        maxLevelIndex = stats.levels.Length - 1;
    }

    protected virtual int InitialLevelIndex => 0;

    public virtual GameObject NextLevelPrefab => null;

    protected TowerLevel GetLevelData(int index)
    {
        if (stats == null || stats.levels == null)
        {
            return null;
        }

        if (index < 0 || index >= stats.levels.Length)
        {
            return null;
        }

        return stats.levels[index];
    }

    public int GetBuildCost()
    {
        var data = GetLevelData(InitialLevelIndex);
        return data != null ? data.cost : 0;
    }

    public int GetUpgradeCost()
    {
        var next = NextLevelPrefab;
        if (next == null)
        {
            return 0;
        }

        var controller = next.GetComponent<TowerController>();
        if (controller == null)
        {
            return 0;
        }

        return controller.GetBuildCost();
    }

    public int GetSellValue()
    {
        return sellRefund;
    }
}
