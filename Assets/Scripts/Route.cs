using UnityEngine;

public class Route : MonoBehaviour
{
    public Transform[] waypoints;

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        for (int i = 0; i < waypoints.Length - 1; i++)
        {
            Gizmos.DrawLine(
                waypoints[i].position,
                waypoints[i + 1].position);
        }
    }
}