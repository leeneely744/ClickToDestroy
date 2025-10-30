using UnityEngine;
using UnityEngine.InputSystem;

public class NewMonoBehaviourScript : MonoBehaviour
{
    private Rigidbody2D rb;
    private int speed = 50;

    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
    }
    
    void Update() {
        Vector2 pos = rb.position;

        if (Keyboard.current.upArrowKey.isPressed)
            pos += Vector2.up * speed * Time.deltaTime;
        if (Keyboard.current.downArrowKey.isPressed)
            pos += Vector2.down * speed * Time.deltaTime;
        if (Keyboard.current.rightArrowKey.isPressed)
            pos += Vector2.right * speed * Time.deltaTime;
        if (Keyboard.current.leftArrowKey.isPressed)
            pos += Vector2.left * speed * Time.deltaTime;

        rb.MovePosition(pos);
    }
}
