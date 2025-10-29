using UnityEngine;
using UnityEngine.InputSystem;

public class NewMonoBehaviourScript : MonoBehaviour
{
    private Vector2Int m_position;
    void Start()
    {
        m_position = new Vector2Int(1, 1);

    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            m_position += Vector2Int.up;
            transform.position = new Vector3(transform.position.x, m_position.y, transform.position.z);
        }

        if (Keyboard.current.downArrowKey.wasPressedThisFrame)
        {
            m_position += Vector2Int.down;
            transform.position = new Vector3(transform.position.x, m_position.y, transform.position.z);
        }

        if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
        {
            m_position += Vector2Int.left;
            transform.position = new Vector3(m_position.x, transform.position.y, transform.position.z);
        }

        if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            m_position += Vector2Int.right;
            transform.position = new Vector3(m_position.x, transform.position.y, transform.position.z);
        }
    }
}
