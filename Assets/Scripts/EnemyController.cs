using UnityEngine;

public class EnemyController : MonoBehaviour
{
    private Rigidbody2D rb;
    private ScoreBoard scoreBoard;

    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
        scoreBoard = GameObject.Find("ScoreBoard")?.GetComponent<ScoreBoard>();
        if (scoreBoard == null)
        {
            Debug.LogError("ScoreBoard not found");
        }
    }

    void FixedUpdate()
    {
        Vector2 pos = rb.position;

        pos += Vector2.right * 2 * Time.deltaTime;

        if (pos.x > 10)
        {
            Destroy(gameObject);
        }

        rb.MovePosition(pos);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"Collision with {collision.gameObject.tag}");
        if (collision.gameObject.tag == "Goal")
        {
            Destroy(gameObject);
            scoreBoard.CalcHp(10);
        }
    }
}
