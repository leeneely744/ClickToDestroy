using UnityEngine;

public class MovePoint : MonoBehaviour
{
    public Vector3[] points;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Vector3 GetMovePointPosition(int index)
    {
        return points[index];
    }

    private void OnDrawGizmos()
    {
        for (int i = 0; i < points.Length; i++)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(points[i], 0.5f);
        }
    }
}
