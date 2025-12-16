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

    public int hp = 30;
    public int rewardMoney = 20;

    private bool hasRemovedFromSpawner;

    public bool IsDead => hp <= 0;

    private Animator animator;
    private EnemyAttackController attackController;

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
    }

    private void FixedUpdate()
    {
        if (waypoints == null || waypoints.Length == 0)
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

    public void SetRoute(Route route)
    {
        waypoints = route.waypoints;
        transform.position = waypoints[0].position;
    }

    public void SetSpawner(EnemySpawner spawner)
    {
        ownerSpawner = spawner;
    }

    public void TakeDamage(int damage)
    {
        hp -= damage;
        if (hp <= 0)
        {
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
}
