using TD.Spawning;
using UnityEngine;

namespace TD.SpawningEditor
{
    /// <summary>
    /// WaveAsset に対するバルク操作（時刻シフト、件数スケールなど）。
    /// 副作用のみで戻り値は持たず、呼び出し側で Undo 記録と SetDirty を行うこと。
    /// </summary>
    public static class WaveBulkOps
    {
        public static void ShiftStart(WaveAsset wave, float dt)
        {
            if (wave == null || wave.groups == null) return;
            foreach (var g in wave.groups)
            {
                if (g == null) continue;
                g.startTime = Mathf.Max(0f, g.startTime + dt);
            }
        }

        public static void ScaleCount(WaveAsset wave, float k)
        {
            if (wave == null || wave.groups == null) return;
            foreach (var g in wave.groups)
            {
                if (g == null) continue;
                g.count = Mathf.Max(1, Mathf.RoundToInt(g.count * k));
            }
        }

        public static void ScaleInterval(WaveAsset wave, float k)
        {
            if (wave == null || wave.groups == null) return;
            foreach (var g in wave.groups)
            {
                if (g == null) continue;
                g.interval = Mathf.Max(0f, g.interval * k);
            }
        }

        /// <summary>
        /// SpawnGroup を 1 体ずつのスポーンに展開して時刻順に並べた一覧を返す。
        /// タイムライン表示やソートに使う。
        /// </summary>
        public static System.Collections.Generic.List<float> ExpandSpawnTimes(WaveAsset wave)
        {
            var result = new System.Collections.Generic.List<float>();
            if (wave == null || wave.groups == null) return result;
            foreach (var g in wave.groups)
            {
                if (g == null) continue;
                int count = Mathf.Max(1, g.count);
                float interval = Mathf.Max(0f, g.interval);
                for (int i = 0; i < count; i++)
                {
                    result.Add(g.startTime + interval * i);
                }
            }
            result.Sort();
            return result;
        }
    }
}
