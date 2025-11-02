using UnityEngine;

public class EnemyController : MonoBehaviour
{
    private Rigidbody2D rb;

    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
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
}
