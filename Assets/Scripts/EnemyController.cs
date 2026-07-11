using UnityEngine;

public class EnemyController : MonoBehaviour, IStatusProvider
{
    private ScoreBoard scoreBoard;
    private Money moneyController;
    private EnemySpawner ownerSpawner;

    private Transform[] waypoints;
    private int currentWaypointIndex = 0;
    private float speed = 2.0f;
    private Vector3 initialScale;

    private int hp = 30;
    private int rewardMoney = 20;
    private bool isFlying = false;

    private bool hasRemovedFromSpawner;

    public bool IsDead => hp <= 0;
    public bool IsFlying => isFlying;

    /// <summary>
    /// 敵の定義データ。プレハブの状態でも参照できる（チュートリアルの飛行敵判定などに使用）。
    /// </summary>
    public EnemyData Data => enemyData;

    private Animator animator;
    private EnemyAttackController attackController;
    [SerializeField] private HealthBarController healthBar;
    [SerializeField] private EnemyData enemyData;

    [Tooltip("死亡アニメーションを再生してから GameObject を破棄するまでの秒数")]
    [SerializeField] private float deathAnimationDuration = 1f;

    private int maxHp;

    private void Start()
    {
        scoreBoard = FindAnyObjectByType<ScoreBoard>();
        if (scoreBoard == null)
        {
            Debug.LogError("ScoreBoard not found");
        }

        moneyController = FindAnyObjectByType<Money>();
        if (moneyController == null)
        {
            Debug.LogError("Money controller not found");
        }

        // Remember initial scale so we can flip left/right without resetting the intended size
        initialScale = transform.localScale;

        animator = GetComponent<Animator>();
        attackController = GetComponent<EnemyAttackController>();

        // ScriptableObject が設定されていれば、そこから各種ステータスを初期化
        if (enemyData != null)
        {
            maxHp = enemyData.maxHp;
            hp = enemyData.maxHp;
            speed = enemyData.moveSpeed;
            isFlying = enemyData.isFlying;
            rewardMoney = enemyData.rewardMoney;

            // 攻撃関連も EnemyAttackController に適用
            if (attackController != null)
            {
                attackController.ApplyData(enemyData);
            }

            // アニメーションのオーバーライドが指定されていれば適用
            if (animator != null && enemyData.animatorOverride != null)
            {
                animator.runtimeAnimatorController = enemyData.animatorOverride;
            }
        }
        else
        {
            maxHp = hp;
        }

        if (healthBar == null)
        {
            healthBar = GetComponentInChildren<HealthBarController>();
        }

        UpdateHealthBar();
        RegisterClickHandlers();
    }

    private void RegisterClickHandlers()
    {
        foreach (var col in GetComponentsInChildren<Collider2D>())
        {
            if (col.GetComponent<StatusClickHandler>() == null)
                col.gameObject.AddComponent<StatusClickHandler>();
        }
    }

    public StatusInfo GetStatusInfo()
    {
        var sr = GetComponentInChildren<SpriteRenderer>();
        int? atk = null;
        float? physDef = null;
        float? magDef = null;
        if (enemyData != null)
        {
            int maxDmg = Mathf.Max(enemyData.meleeAttack.damage, enemyData.rangedAttack.damage);
            if (maxDmg > 0) atk = maxDmg;
            physDef = enemyData.physicalResistance;
            magDef = enemyData.magicalResistance;
        }

        return new StatusInfo
        {
            displayName = gameObject.name.Replace("(Clone)", "").Trim(),
            icon = sr != null ? sr.sprite : null,
            maxHp = maxHp,
            getCurrentHp = () => hp,
            attackDamage = atk,
            physicalResistance = physDef,
            magicalResistance = magDef,
        };
    }

    private void FixedUpdate()
    {
        if (waypoints == null || waypoints.Length == 0)
        {
            return;
        }
        if (currentWaypointIndex >= waypoints.Length)
        {
            return;
        }

        // 交戦中（攻撃コンポーネントが付いていて交戦状態）のときは移動しない
        bool isEngaged = attackController != null && attackController.IsEngaged;

        if (!isEngaged)
        {
            transform.position = Vector2.MoveTowards(
                transform.position,
                waypoints[currentWaypointIndex].position,
                speed * Time.deltaTime
            );
        }

        if (Vector2.Distance(transform.position, waypoints[currentWaypointIndex].position) < 0.05f)
        {
            currentWaypointIndex++;

            if (currentWaypointIndex < waypoints.Length)
            {
                float direction = waypoints[currentWaypointIndex].position.x > transform.position.x ? 1f : -1f;
                transform.localScale = new Vector3(Mathf.Abs(initialScale.x) * direction, initialScale.y, initialScale.z);
            }

            if (currentWaypointIndex >= waypoints.Length)
            {
                scoreBoard?.CalcHp(10);
                NotifySpawnerRemoved();
                Destroy(gameObject);
            }
        }
    }

    /// <summary>
    /// ルートを設定する。無効なルートの場合は false を返す。
    /// 呼び出し側（EnemySpawner）は false のときこの敵を破棄してカウント対象から外すこと。
    /// </summary>
    public bool SetRoute(Route route)
    {
        if (route == null || route.waypoints == null || route.waypoints.Length == 0)
        {
            Debug.LogError($"[EnemyController] SetRoute: route または waypoints が無効です。route={route?.name ?? "null"}", this);
            return false;
        }
        waypoints = route.waypoints;
        transform.position = waypoints[0].position;
        return true;
    }

    public void SetSpawner(EnemySpawner spawner)
    {
        ownerSpawner = spawner;
    }

    public void TakeDamage(int damage, AttackType type = AttackType.Physical)
    {
        // 死亡演出中の多重ダメージ（報酬の二重付与など）を防ぐ
        if (IsDead)
        {
            return;
        }

        float resistance = type == AttackType.Physical
            ? (enemyData != null ? enemyData.physicalResistance : 0f)
            : (enemyData != null ? enemyData.magicalResistance : 0f);
        int finalDamage = Mathf.Max(1, Mathf.RoundToInt(damage * (1f - resistance)));
        hp -= finalDamage;
        UpdateHealthBar();
        if (hp <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// 死亡処理。報酬付与・スポナー通知のあと、コライダーと移動/攻撃を止めて
    /// 死亡アニメーションを再生し、遅延付きで GameObject を破棄する。
    /// </summary>
    private void Die()
    {
        moneyController?.AddMoney(rewardMoney);
        NotifySpawnerRemoved();

        // タワーや兵士のターゲット・クリック判定から外れるようにコライダーを無効化
        foreach (var col in GetComponentsInChildren<Collider2D>())
        {
            col.enabled = false;
        }

        // 交戦・移動を停止（enabled = false で FixedUpdate を止める）
        if (attackController != null)
        {
            attackController.Disengage();
            attackController.enabled = false;
        }
        enabled = false;

        if (animator != null)
        {
            animator.SetTrigger("Die");
            Destroy(gameObject, deathAnimationDuration);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 防衛ユニットとの交戦を開始する。
    /// 実際の攻撃ロジックは EnemyAttackController に委譲される。
    /// </summary>
    public void EngageDefender(IDefender defender)
    {
        if (attackController == null)
        {
            return;
        }

        attackController.EngageDefender(defender);
    }

    /// <summary>
    /// 防衛ユニットとの交戦を終了する。
    /// </summary>
    public void DisengageDefender()
    {
        if (attackController == null)
        {
            return;
        }

        attackController.Disengage();
    }

    private void NotifySpawnerRemoved()
    {
        if (hasRemovedFromSpawner)
        {
            return;
        }

        ownerSpawner?.NotifyEnemyRemoved(this);
        hasRemovedFromSpawner = true;
    }

    private void UpdateHealthBar()
    {
        if (healthBar == null)
        {
            return;
        }

        float ratio = maxHp > 0 ? Mathf.Clamp01((float)hp / maxHp) : 0f;
        healthBar.SetRatio(ratio);
    }
}
