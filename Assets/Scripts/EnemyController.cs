using UnityEngine;

public class EnemyController : MonoBehaviour
{
    // private Rigidbody2D rb;
    private ScoreBoard scoreBoard;

    private Transform[] waypoints;
    private int currentWaypointIndex = 0;
    private float speed = 2.0f;

    public int hp = 30;

    void Start()
    {
        scoreBoard = GameObject.Find("ScoreBoard")?.GetComponent<ScoreBoard>();
        if (scoreBoard == null)
        {
            Debug.LogError("ScoreBoard not found");
        }
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

        // 次のポイントへ向かう
        if (Vector2.Distance(transform.position, waypoints[currentWaypointIndex].position) < 0.05f)
        {
            currentWaypointIndex++;

            // 現在のポイントよりも次のポイントのx座標が大きい場合、localScale.xを逆にする（デフォルトは左向きを想定）
            if (currentWaypointIndex < waypoints.Length)
            {
                if (waypoints[currentWaypointIndex].position.x > transform.position.x)
                {
                    transform.localScale = new Vector3(-1, 1, 1);
                }
                else
                {
                    transform.localScale = new Vector3(1, 1, 1);
                }
            }

            // 次のポイントがない場合はゴール
            if (currentWaypointIndex >= waypoints.Length)
            {
                Destroy(gameObject);
                scoreBoard.CalcHp(10);
            }
        }
    }

    public void SetRoute(Route route)
    {
        waypoints = route.waypoints;
        transform.position = waypoints[0].position;
    }

    public void TakeDamage(int damage)
    {
        hp -= damage;
        if (hp <= 0)
        {
            Destroy(gameObject);
        }
    }
}
