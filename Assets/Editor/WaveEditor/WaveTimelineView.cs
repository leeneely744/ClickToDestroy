using TD.Spawning;
using UnityEditor;
using UnityEngine;

namespace TD.SpawningEditor
{
    /// <summary>
    /// 1 つの WaveAsset を横方向タイムラインで描画する。
    /// グループごとに色分けされたレーンを縦に並べ、count/interval に基づき個別マーカーを打つ。
    /// </summary>
    public static class WaveTimelineView
    {
        private static readonly Color[] Palette =
        {
            new Color(0.40f, 0.80f, 1.00f),
            new Color(1.00f, 0.55f, 0.40f),
            new Color(0.65f, 0.95f, 0.55f),
            new Color(1.00f, 0.85f, 0.40f),
            new Color(0.85f, 0.55f, 1.00f),
            new Color(0.55f, 0.95f, 0.95f),
            new Color(1.00f, 0.65f, 0.85f),
        };

        public static void Draw(WaveAsset wave, float height = 120f)
        {
            EditorGUILayout.LabelField("Timeline", EditorStyles.boldLabel);
            var rect = GUILayoutUtility.GetRect(0, height, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(rect, new Color(0.13f, 0.13f, 0.16f));

            if (wave == null || wave.groups == null || wave.groups.Length == 0)
            {
                var center = new GUIStyle(EditorStyles.miniLabel)
                {
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = new Color(1, 1, 1, 0.4f) },
                };
                GUI.Label(rect, "(no spawn groups)", center);
                return;
            }

            float maxT = 1f;
            foreach (var g in wave.groups)
            {
                if (g == null) continue;
                int count = Mathf.Max(1, g.count);
                float interval = Mathf.Max(0f, g.interval);
                maxT = Mathf.Max(maxT, g.startTime + interval * (count - 1));
            }
            // 右側に少し余白を取る
            maxT = Mathf.Max(1f, maxT) + 1f;

            DrawTimeGrid(rect, maxT);
            DrawLanes(rect, wave, maxT);
        }

        private static void DrawTimeGrid(Rect rect, float maxT)
        {
            var grid = new Color(1, 1, 1, 0.07f);
            var gridStrong = new Color(1, 1, 1, 0.18f);
            var labelStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                normal = { textColor = new Color(1, 1, 1, 0.5f) },
            };

            float pxPerSec = rect.width / maxT;

            // 1 秒刻みで線とラベル。時間軸が長くなるとラベルを間引き。
            int labelEvery = pxPerSec >= 60f ? 1 : pxPerSec >= 30f ? 2 : pxPerSec >= 15f ? 5 : 10;
            int max = Mathf.CeilToInt(maxT);
            for (int s = 0; s <= max; s++)
            {
                float x = rect.x + s * pxPerSec;
                EditorGUI.DrawRect(new Rect(x, rect.y, 1, rect.height), s % labelEvery == 0 ? gridStrong : grid);
                if (s % labelEvery == 0)
                {
                    GUI.Label(new Rect(x + 2, rect.y, 40, 14), s + "s", labelStyle);
                }
            }
        }

        private static void DrawLanes(Rect rect, WaveAsset wave, float maxT)
        {
            int laneCount = wave.groups.Length;
            float topPadding = 18f;
            float bottomPadding = 4f;
            float laneHeight = (rect.height - topPadding - bottomPadding) / Mathf.Max(1, laneCount);

            for (int gi = 0; gi < laneCount; gi++)
            {
                var g = wave.groups[gi];
                if (g == null) continue;

                int count = Mathf.Max(1, g.count);
                float interval = Mathf.Max(0f, g.interval);
                var color = Palette[gi % Palette.Length];

                float laneY = rect.y + topPadding + laneHeight * gi;
                // レーン背景を薄く塗る
                EditorGUI.DrawRect(
                    new Rect(rect.x, laneY, rect.width, laneHeight),
                    new Color(color.r, color.g, color.b, 0.06f));

                for (int i = 0; i < count; i++)
                {
                    float t = g.startTime + interval * i;
                    float x = rect.x + (t / maxT) * rect.width;
                    var marker = new Rect(x - 2, laneY + 2, 4, Mathf.Max(6, laneHeight - 4));
                    EditorGUI.DrawRect(marker, color);
                }

                // 左端にラベル
                var enemyName = g.enemyPrefab != null ? g.enemyPrefab.name : "(no prefab)";
                var routeName = g.route != null ? g.route.name : "(no route)";
                var labelStyle = new GUIStyle(EditorStyles.miniLabel)
                {
                    normal = { textColor = color },
                };
                GUI.Label(new Rect(rect.x + 4, laneY, 220, 14), $"{enemyName} → {routeName}", labelStyle);
            }
        }
    }
}
