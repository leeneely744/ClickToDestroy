using UnityEngine;
using UnityEngine.InputSystem;

public class NewMonoBehaviourScript : MonoBehaviour
{
    private Rigidbody2D rb;

    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
    }
    
    void Update() {
        // Upキーで前に進む
        if (Keyboard.current.upArrowKey.wasPressedThisFrame) {
            rb.MovePosition(transform.position + transform.forward * 10f * Time.deltaTime);
        }
        // Downキーで後ろに進む
        if (Keyboard.current.downArrowKey.wasPressedThisFrame) {
            rb.MovePosition(transform.position - transform.forward * 10f * Time.deltaTime);
        }
        //right キーで右に進む
        if (Keyboard.current.rightArrowKey.wasPressedThisFrame) {
            rb.MovePosition(transform.position + transform.right * 10f * Time.deltaTime);
        }
        //left キーで左に進む
        if (Keyboard.current.leftArrowKey.wasPressedThisFrame) {
            rb.MovePosition(transform.position - transform.right * 10f * Time.deltaTime);
        }
    }
}
