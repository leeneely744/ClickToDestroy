using UnityEngine;

public class EnemyController : MonoBehaviour
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

    private Animator animator;
    private EnemyAttackController attackController;
    [SerializeField] private HealthBarController healthBar;
    [SerializeField] private EnemyData enemyData;
    private int maxHp;

    // ===== 経路追従のゆらぎ（重なり防止） =====
    [Header("経路追従のゆらぎ")]
    [Tooltip("経路に対して垂直方向にどれだけずれるか（ワールド単位）。\n" +
             "個体ごとに ±この値の範囲で1つ固定され、複数のEnemyが同じ経路を通っても重なりにくくなる。\n" +
             "0にすると全Enemyが厳密に経路上を移動する。")]
    [SerializeField] private float maxLateralOffset = 0.25f;

    // 個体ごとに固定される値（Awakeで確定）
    private float lateralOffset;

    private void Awake()
    {
        // 経路と垂直な固定オフセットを個体ごとに決める
        lateralOffset = Random.Range(-maxLateralOffset, maxLateralOffset);
    }

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

        // 現在のWPに、経路と垂直方向の固定オフセットを適用した目標位置
        Vector3 offsetTarget = ComputeOffsetWaypoint(currentWaypointIndex);

        // 交戦中（攻撃コンポーネントが付いていて交戦状態）のときは移動しない
        bool isEngaged = attackController != null && attackController.IsEngaged;

        if (!isEngaged)
        {
            transform.position = Vector2.MoveTowards(
                transform.position,
                offsetTarget,
                speed * Time.deltaTime
            );
        }

        if (Vector2.Distance(transform.position, offsetTarget) < 0.05f)
        {
            currentWaypointIndex++;

            if (currentWaypointIndex < waypoints.Length)
            {
                float direction = waypoints[currentWaypointIndex].position.x > transform.position.x ? 1f : -1f;
                transform.localScale = new Vector3(Mathf.Abs(initialScale.x) * direction, initialScale.y, initialScale.z);
            }

            if (currentWaypointIndex >= waypoints.Length)
            {
                Debug.Log($"[EnemyController] {gameObject.name} がゴールに到達。生存フレーム数でのindex={currentWaypointIndex}, waypoints={waypoints.Length}", this);
                scoreBoard?.CalcHp(10);
                NotifySpawnerRemoved();
                Destroy(gameObject);
            }
        }
    }

    public void SetRoute(Route route)
    {
        if (route == null || route.waypoints == null || route.waypoints.Length == 0)
        {
            Debug.LogError($"[EnemyController] SetRoute: route または waypoints が無効です。route={route?.name ?? "null"}", this);
            return;
        }
        Debug.Log($"[EnemyController] SetRoute: {route.name} waypoints={route.waypoints.Length}", this);
        waypoints = route.waypoints;
        // 開始位置もWP0に対して垂直方向のオフセットを掛けた位置にする（重なり対策）
        transform.position = ComputeOffsetWaypoint(0);
    }

    /// <summary>
    /// 指定したWPに対して、経路と垂直方向の固定オフセットを足した位置を返す。
    /// 個体ごとの lateralOffset によって左右にずれる。
    /// </summary>
    private Vector3 ComputeOffsetWaypoint(int index)
    {
        if (waypoints == null || index < 0 || index >= waypoints.Length || waypoints[index] == null)
        {
            return transform.position;
        }
        Vector3 wp = waypoints[index].position;
        Vector3 segDir = GetSegmentDirection(index);
        Vector3 perp = new Vector3(-segDir.y, segDir.x, 0f);
        return wp + perp * lateralOffset;
    }

    /// <summary>
    /// 指定したWPでの進行方向（次のWPへの単位ベクトル）を返す。
    /// 最終WPの場合は1つ前のWPからの向きを使う。
    /// </summary>
    private Vector3 GetSegmentDirection(int index)
    {
        if (waypoints == null || waypoints.Length == 0)
        {
            return Vector3.right;
        }

        if (index < waypoints.Length - 1
            && waypoints[index] != null
            && waypoints[index + 1] != null)
        {
            Vector3 to = waypoints[index + 1].position - waypoints[index].position;
            if (to.sqrMagnitude > 0.0001f)
            {
                return to.normalized;
            }
        }

        if (index > 0
            && waypoints[index] != null
            && waypoints[index - 1] != null)
        {
            Vector3 to = waypoints[index].position - waypoints[index - 1].position;
            if (to.sqrMagnitude > 0.0001f)
            {
                return to.normalized;
            }
        }

        return Vector3.right;
    }

    public void SetSpawner(EnemySpawner spawner)
    {
        ownerSpawner = spawner;
    }

    public void TakeDamage(int damage)
    {
        hp -= damage;
        UpdateHealthBar();
        if (hp <= 0)
        {
            Debug.Log($"[EnemyController] {gameObject.name} が死亡（TakeDamage）", this);
            moneyController?.AddMoney(rewardMoney);
            NotifySpawnerRemoved();

            if (animator != null)
            {
                animator.SetTrigger("Die");
            }

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
