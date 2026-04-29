using TD.Spawning;
using UnityEngine;

public class Route : MonoBehaviour
{
    [Tooltip("この Route の識別子となる RouteAsset。WaveAsset 側はこの RouteAsset を参照する。")]
    [SerializeField] private RouteAsset asset;

    public RouteAsset Asset => asset;

    public Transform[] waypoints;

    private void OnEnable()
    {
        if (asset != null)
        {
            RouteRegistry.Register(asset, this);
        }
        else
        {
            Debug.LogWarning(
                $"Route '{name}' に RouteAsset が割り当てられていません。" +
                "Tools > TD > Bind Scene Routes to RouteAssets を実行してください。",
                this);
        }
    }

    private void OnDisable()
    {
        if (asset != null)
        {
            RouteRegistry.Unregister(asset, this);
        }
    }

    void OnDrawGizmos()
    {
        if (waypoints == null) return;
        Gizmos.color = Color.red;
        for (int i = 0; i < waypoints.Length - 1; i++)
        {
            if (waypoints[i] == null || waypoints[i + 1] == null) continue;
            Gizmos.DrawLine(
                waypoints[i].position,
                waypoints[i + 1].position);
        }
    }

#if UNITY_EDITOR
    public void EditorAssignRouteAsset(RouteAsset newAsset)
    {
        asset = newAsset;
    }
#endif
}
