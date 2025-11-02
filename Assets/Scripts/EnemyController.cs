using UnityEngine;

public class EnemyController : MonoBehaviour
{
    private Rigidbody2D rb;

    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        Vector2 pos = rb.position;

        pos += Vector2.right * 30 * Time.deltaTime;

        rb.MovePosition(pos);
    }
}
