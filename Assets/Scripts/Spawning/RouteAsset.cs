using UnityEngine;

namespace TD.Spawning
{
    /// <summary>
    /// シーン上の Route の「識別子」となる ScriptableObject。
    /// WaveAsset (アセット) からシーン上の Route (MonoBehaviour) を直接参照することはできないため、
    /// このアセットを介して間接参照する。
    /// 実行時には RouteRegistry によって、同じ RouteAsset を持つ Route と紐づけられる。
    /// </summary>
    [CreateAssetMenu(menuName = "TD/Route", fileName = "Route")]
    public class RouteAsset : ScriptableObject
    {
        [TextArea]
        public string description;
    }
}
