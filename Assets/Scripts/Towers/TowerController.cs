using UnityEngine;
using System.Collections.Generic;
using Tags = Constants.Tags;

public class TowerController : MonoBehaviour
{
    private CircleCollider2D rangeCollider;
    private List<EnemyController> enemiesInRange = new List<EnemyController>();

    public GameObject projectilePrefab;
    public Transform firePoint;  // 砲弾の発射位置

    [Tooltip("0 より大きい場合、弾はこの時間でターゲットへ到達できる速度に調整されます。")]
    public float projectileTravelTime = 0f;

    private bool isSelected = false;
    private SpriteRenderer attackRangeRenderer;
    protected Money moneyController;
    private TowerPlace towerPlace;
    public int levelIndex = 0;

    /// <summary>
    /// TowerStats
    /// </summary>
    [SerializeField] protected TowerStats stats;
    private int maxLevelIndex;
    private string towerName;
    [System.NonSerialized] public int cost;
    private int sellRefund;
    private float attackInterval;
    private float range;

    public TowerStats Stats => stats;
    protected float AttackRange => range;
    protected float AttackRangeWorldRadius =>
        rangeCollider != null
            ? rangeCollider.radius * rangeCollider.transform.lossyScale.x
            : range;
    public float AttackInterval => attackInterval;
    protected TowerPlace CurrentTowerPlace => towerPlace;

    protected virtual void Start()
    {
        Transform rangeCircle = transform.Find("AttackRangeCircle");
        if (rangeCircle == null)
        {
            Debug.LogWarning($"AttackRangeCircle not found on {name}");
        }
        else
        {
            attackRangeRenderer = rangeCircle.GetComponent<SpriteRenderer>();
            if (attackRangeRenderer == null)
            {
                Debug.LogWarning($"SpriteRenderer not found on AttackRangeCircle of {name}");
            }

            rangeCollider = rangeCircle.GetComponent<CircleCollider2D>();
            if (rangeCollider == null)
            {
                Debug.LogWarning($"CircleCollider2D not found on AttackRangeCircle of {name}");
            }
        }

        if (attackRangeRenderer != null)
        {
            attackRangeRenderer.enabled = false;
        }

        moneyController = FindAnyObjectByType<Money>();
        if (moneyController == null)
        {
            Debug.LogError("Money controller not found");
        }

        // レベルインデックスの決定
        // - Bow のように「同じコントローラ＋Prefabごとに levelIndex を変える」ケースでは
        //   インスペクタで設定された levelIndex をそのまま使う。
        // - MagicTower2 など、サブクラス側で InitialLevelIndex を上書きしているケースでは
        //   デフォルト値 0 のままなので、ここで InitialLevelIndex を採用する。
        if (levelIndex == 0)
        {
            levelIndex = InitialLevelIndex;
        }

        ApplyStats(levelIndex);

        var attackComponent = GetComponent<TowerAttackController>();
        if (attackComponent != null)
        {
            attackComponent.Configure(attackInterval, range, projectilePrefab, firePoint, projectileTravelTime);
        }
    }

    protected void ApplyStats(int index)
    {
        var data = GetLevelData(index);
        if (data == null)
        {
            Debug.LogError($"Tower level data not found for index {index}");
            return;
        }
        towerName = data.towerName;
        cost = data.cost;
        sellRefund = data.sellRefund;
        attackInterval = data.attackInterval;
        range = data.range;

        maxLevelIndex = stats.levels.Length - 1;
        levelIndex = index;
    }

    protected virtual void Update()
    {
        // GuardianTowerControllerBase でオーバーライドされる
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

    private void PlayAttackAnimation()
    {
        // ひとまず従来どおり、子オブジェクトについている ArcherAnimatorController に委譲するだけに戻す
        var legacyArcher = GetComponentInChildren<ArcherAnimatorController>();
        if (legacyArcher != null)
        {
            legacyArcher.PlayAttack();
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

    public virtual bool UpgradeTower()
    {
        var nextPrefab = NextLevelPrefab;
        if (nextPrefab == null || towerPlace == null)
        {
            Debug.LogWarning($"[TowerController] Cannot upgrade {name}: nextPrefab={(nextPrefab == null ? "null" : nextPrefab.name)}, towerPlace={(towerPlace == null ? "null" : towerPlace.name)}");
            return false;
        }

        int upgradeCost = GetUpgradeCost();
        if (moneyController != null && upgradeCost > 0)
        {
            if (!moneyController.SpendMoney(upgradeCost))
            {
                Debug.Log("Not enough money to upgrade tower.");
                Debug.LogWarning($"[TowerController] Upgrade failed (lack of money) {name}: cost={upgradeCost}, money={moneyController.CurrentMoney}");
                return false;
            }
        }

        var newTowerObj = Instantiate(nextPrefab, transform.position, Quaternion.identity);
        var newController = newTowerObj.GetComponent<TowerController>();
        if (newController != null)
        {
            newController.SetTowerPlace(towerPlace);
            OnUpgradeTo(newController);
        }
        else
        {
            Debug.LogError($"[TowerController] Next prefab {nextPrefab.name} does not have TowerController");
        }

        Destroy(gameObject);
        return true;
    }

    // GuardianTowerControllerBase でオーバーライドされる。
    // savedGuardianCenter を引き継ぐための処理。
    protected virtual void OnUpgradeTo(TowerController newController) { }

    protected virtual int InitialLevelIndex => 0;

    public virtual GameObject NextLevelPrefab
    {
        get
        {
            var data = GetLevelData(levelIndex);
            return data != null ? data.nextLevelPrefab : null;
        }
    }

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

    public virtual int GetUpgradeCost()
    {
        // まず TowerStats の「次レベル」のコストを優先して参照する
        if (stats != null && stats.levels != null)
        {
            int nextIndex = levelIndex + 1;
            var nextLevelData = GetLevelData(nextIndex);
            if (nextLevelData != null)
            {
                return nextLevelData.cost;
            }
        }

        // TowerStats から取得できなかった場合は、従来どおり nextPrefab の BuildCost にフォールバック
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

    public virtual int GetMaxUnits()
    {
        return 0;
    }

    public void DestroyTower()
    {
        towerPlace.SetOccupied(false);
        Destroy(gameObject);
    }

    /// <summary>
    /// このタワーが紐づいている TowerPlace を返す。
    /// （未設定の場合は null を返す）
    /// </summary>
    public TowerPlace GetTowerPlace()
    {
        return towerPlace;
    }

}
