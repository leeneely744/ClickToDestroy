using System.Collections.Generic;
using UnityEngine;

namespace TD.Spawning
{
    /// <summary>
    /// RouteAsset (識別子) ⇄ Route (シーン上の実体) を紐づける静的レジストリ。
    /// Route が OnEnable のタイミングで自身を登録し、OnDisable で解除する。
    /// </summary>
    public static class RouteRegistry
    {
        private static readonly Dictionary<RouteAsset, Route> map = new Dictionary<RouteAsset, Route>();

        // Play 開始時に確実にクリアされるようにしておく（Domain Reload 無効化対策も兼ねる）。
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ClearOnLoad()
        {
            map.Clear();
        }

        public static void Register(RouteAsset asset, Route route)
        {
            if (asset == null || route == null) return;

            if (map.TryGetValue(asset, out var existing) && existing != null && existing != route)
            {
                Debug.LogWarning(
                    $"RouteRegistry: '{asset.name}' は既に '{existing.name}' にバインド済みです。" +
                    $"'{route.name}' で上書きします。RouteAsset がシーン内で重複していないか確認してください。",
                    route);
            }
            map[asset] = route;
        }

        public static void Unregister(RouteAsset asset, Route route)
        {
            if (asset == null) return;
            if (map.TryGetValue(asset, out var existing) && existing == route)
            {
                map.Remove(asset);
            }
        }

        public static Route Resolve(RouteAsset asset)
        {
            if (asset == null) return null;
            map.TryGetValue(asset, out var route);
            return route;
        }
    }
}
