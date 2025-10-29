using UnityEngine;
using UnityEngine.InputSystem;

public class NewMonoBehaviourScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            Debug.Log("Up arrow key was pressed.");
        }

        if (Keyboard.current.downArrowKey.wasPressedThisFrame)
        {
            Debug.Log("Down arrow key was pressed.");
        }

        if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
        {
            Debug.Log("Left arrow key was pressed.");
        }

        if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            Debug.Log("Right arrow key was pressed.");
        }
    }
}
