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

    void Start()
    {
        scoreBoard = FindObjectOfType<ScoreBoard>();
        if (scoreBoard == null)
        {
            Debug.LogError("ScoreBoard not found");
        }

        moneyController = FindObjectOfType<Money>();
        if (moneyController == null)
        {
            Debug.LogError("Money controller not found");
        }

        // Remember initial scale so we can flip left/right without resetting the intended size
        initialScale = transform.localScale;
    }

    void FixedUpdate()
    {
        if (waypoints == null || waypoints.Length == 0)
        {
            return;
        }

        transform.position = Vector2.MoveTowards(
            transform.position,
            waypoints[currentWaypointIndex].position,
            speed * Time.deltaTime
        );

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
            Destroy(gameObject);
        }
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
