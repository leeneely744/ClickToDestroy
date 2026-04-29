using System.Collections.Generic;
using System.IO;
using TD.Spawning;
using UnityEditor;
using UnityEngine;

namespace TD.SpawningEditor
{
    public class WaveEditorWindow : EditorWindow
    {
        [SerializeField] private LevelAsset level;
        [SerializeField] private int selectedWaveIndex;
        [SerializeField] private bool bulkAffectsAllWaves;

        private Vector2 tableScroll;
        private Vector2 tabsScroll;

        [MenuItem("Tools/TD/Wave Editor")]
        public static void Open()
        {
            var window = GetWindow<WaveEditorWindow>("Wave Editor");
            window.minSize = new Vector2(820, 520);
            window.Show();
        }

        public static void OpenWith(LevelAsset asset)
        {
            var window = GetWindow<WaveEditorWindow>("Wave Editor");
            window.level = asset;
            window.selectedWaveIndex = 0;
            window.minSize = new Vector2(820, 520);
            window.Repaint();
            window.Show();
        }

        private void OnGUI()
        {
            DrawToolbar();

            if (level == null)
            {
                EditorGUILayout.HelpBox("LevelAsset を上のスロットに割り当ててください。", MessageType.Info);
                return;
            }

            DrawLevelHeader();
            DrawWaveTabs();

            if (level.waves == null || level.waves.Length == 0)
            {
                EditorGUILayout.HelpBox("Wave がありません。Toolbar の [+Wave] で追加してください。", MessageType.Info);
                return;
            }

            selectedWaveIndex = Mathf.Clamp(selectedWaveIndex, 0, level.waves.Length - 1);
            var wave = level.waves[selectedWaveIndex];
            if (wave == null)
            {
                EditorGUILayout.HelpBox(
                    $"Wave {selectedWaveIndex} が null です。Toolbar の [Del] で削除するか、LevelAsset 側で設定し直してください。",
                    MessageType.Warning);
                return;
            }

            DrawWaveHeader(wave);
            EditorGUILayout.Space(2);
            DrawSpawnGroupTable(wave);
            EditorGUILayout.Space(4);
            DrawBulkOps(wave);
            EditorGUILayout.Space(4);
            WaveTimelineView.Draw(wave);
        }

        // ============================================================
        // Toolbar
        // ============================================================

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            EditorGUI.BeginChangeCheck();
            var newLevel = (LevelAsset)EditorGUILayout.ObjectField(
                level, typeof(LevelAsset), false, GUILayout.Width(240));
            if (EditorGUI.EndChangeCheck())
            {
                level = newLevel;
                selectedWaveIndex = 0;
            }

            GUILayout.FlexibleSpace();

            using (new EditorGUI.DisabledScope(level == null))
            {
                if (GUILayout.Button("+Wave", EditorStyles.toolbarButton)) AddWave();
                if (GUILayout.Button("Dup",   EditorStyles.toolbarButton)) DuplicateCurrentWave();
                if (GUILayout.Button("Del",   EditorStyles.toolbarButton)) DeleteCurrentWave();
                if (GUILayout.Button("↑",     EditorStyles.toolbarButton)) MoveCurrentWave(-1);
                if (GUILayout.Button("↓",     EditorStyles.toolbarButton)) MoveCurrentWave(+1);
            }

            EditorGUILayout.EndHorizontal();
        }

        // ============================================================
        // Level / Wave header
        // ============================================================

        private void DrawLevelHeader()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            var newDelay = EditorGUILayout.FloatField(
                "Start Delay", level.startDelay, GUILayout.MaxWidth(280));
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(level, "Edit LevelAsset");
                level.startDelay = newDelay;
                EditorUtility.SetDirty(level);
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField(
                $"Waves: {(level.waves?.Length ?? 0)}",
                GUILayout.Width(80));
            EditorGUILayout.EndHorizontal();
        }

        private void DrawWaveTabs()
        {
            tabsScroll = EditorGUILayout.BeginScrollView(
                tabsScroll, false, false,
                GUILayout.Height(28));
            EditorGUILayout.BeginHorizontal();

            if (level.waves != null)
            {
                for (int i = 0; i < level.waves.Length; i++)
                {
                    var w = level.waves[i];
                    string label = w == null
                        ? $"W{i + 1} (null)"
                        : (string.IsNullOrEmpty(w.waveName) ? $"W{i + 1}" : $"W{i + 1} {w.waveName}");

                    var style = i == selectedWaveIndex
                        ? new GUIStyle(EditorStyles.miniButton) { fontStyle = FontStyle.Bold }
                        : EditorStyles.miniButton;

                    if (GUILayout.Button(label, style, GUILayout.MinWidth(60), GUILayout.Height(22)))
                    {
                        selectedWaveIndex = i;
                        GUI.FocusControl(null);
                    }
                }
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndScrollView();
        }

        private void DrawWaveHeader(WaveAsset wave)
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginChangeCheck();
            var newName = EditorGUILayout.TextField("Wave Name", wave.waveName);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(wave, "Rename Wave");
                wave.waveName = newName;
                EditorUtility.SetDirty(wave);
            }

            EditorGUI.BeginChangeCheck();
            var newDelay = EditorGUILayout.FloatField(
                "Delay before next wave", wave.delayBeforeNextWave, GUILayout.MaxWidth(280));
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(wave, "Edit Wave");
                wave.delayBeforeNextWave = Mathf.Max(0f, newDelay);
                EditorUtility.SetDirty(wave);
            }

            EditorGUILayout.EndHorizontal();
        }

        // ============================================================
        // SpawnGroup table
        // ============================================================

        private void DrawSpawnGroupTable(WaveAsset wave)
        {
            // ヘッダ
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("#",            GUILayout.Width(28));
            GUILayout.Label("Enemy Prefab", GUILayout.Width(180));
            GUILayout.Label("Route",        GUILayout.Width(180));
            GUILayout.Label("Start",        GUILayout.Width(60));
            GUILayout.Label("Count",        GUILayout.Width(50));
            GUILayout.Label("Interval",     GUILayout.Width(70));
            GUILayout.FlexibleSpace();
            GUILayout.Label("Actions",      GUILayout.Width(24 * 4 + 8));
            EditorGUILayout.EndHorizontal();

            tableScroll = EditorGUILayout.BeginScrollView(
                tableScroll, GUILayout.MinHeight(120));

            if (wave.groups == null) wave.groups = new SpawnGroup[0];

            int? toRemove = null;
            int? toDuplicate = null;
            int? toMoveUp = null;
            int? toMoveDown = null;

            for (int i = 0; i < wave.groups.Length; i++)
            {
                var g = wave.groups[i] ??= new SpawnGroup();

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(i.ToString(), GUILayout.Width(28));

                EditorGUI.BeginChangeCheck();
                var prefab   = (GameObject)EditorGUILayout.ObjectField(
                    g.enemyPrefab, typeof(GameObject), false, GUILayout.Width(180));
                var route    = (RouteAsset)EditorGUILayout.ObjectField(
                    g.route, typeof(RouteAsset), false, GUILayout.Width(180));
                var start    = EditorGUILayout.FloatField(g.startTime, GUILayout.Width(60));
                var count    = EditorGUILayout.IntField(g.count, GUILayout.Width(50));
                var interval = EditorGUILayout.FloatField(g.interval, GUILayout.Width(70));
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(wave, "Edit SpawnGroup");
                    g.enemyPrefab = prefab;
                    g.route       = route;
                    g.startTime   = Mathf.Max(0f, start);
                    g.count       = Mathf.Max(1, count);
                    g.interval    = Mathf.Max(0f, interval);
                    EditorUtility.SetDirty(wave);
                }

                GUILayout.FlexibleSpace();
                if (GUILayout.Button("↑", GUILayout.Width(24))) toMoveUp = i;
                if (GUILayout.Button("↓", GUILayout.Width(24))) toMoveDown = i;
                if (GUILayout.Button("D", GUILayout.Width(24))) toDuplicate = i;
                if (GUILayout.Button("X", GUILayout.Width(24))) toRemove = i;

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("+ Add row", GUILayout.Height(22)))
            {
                AddRow(wave);
            }

            if (toRemove.HasValue)    RemoveRow(wave, toRemove.Value);
            if (toDuplicate.HasValue) DuplicateRow(wave, toDuplicate.Value);
            if (toMoveUp.HasValue)    MoveRow(wave, toMoveUp.Value, -1);
            if (toMoveDown.HasValue)  MoveRow(wave, toMoveDown.Value, +1);
        }

        // ============================================================
        // Bulk ops
        // ============================================================

        private void DrawBulkOps(WaveAsset wave)
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("Bulk:", GUILayout.Width(40));
            bulkAffectsAllWaves = GUILayout.Toggle(
                bulkAffectsAllWaves, "All Waves",
                EditorStyles.miniButton, GUILayout.Width(80));

            if (GUILayout.Button("+0.5s",   GUILayout.Width(60)))
                ApplyBulk(wave, w => WaveBulkOps.ShiftStart(w,  0.5f));
            if (GUILayout.Button("-0.5s",   GUILayout.Width(60)))
                ApplyBulk(wave, w => WaveBulkOps.ShiftStart(w, -0.5f));
            if (GUILayout.Button("×1.5 ct", GUILayout.Width(70)))
                ApplyBulk(wave, w => WaveBulkOps.ScaleCount(w, 1.5f));
            if (GUILayout.Button("×0.8 ct", GUILayout.Width(70)))
                ApplyBulk(wave, w => WaveBulkOps.ScaleCount(w, 0.8f));
            if (GUILayout.Button("×1.2 iv", GUILayout.Width(70)))
                ApplyBulk(wave, w => WaveBulkOps.ScaleInterval(w, 1.2f));
            if (GUILayout.Button("×0.8 iv", GUILayout.Width(70)))
                ApplyBulk(wave, w => WaveBulkOps.ScaleInterval(w, 0.8f));

            EditorGUILayout.EndHorizontal();
        }

        private void ApplyBulk(WaveAsset currentWave, System.Action<WaveAsset> op)
        {
            if (bulkAffectsAllWaves)
            {
                if (level.waves == null) return;
                foreach (var w in level.waves)
                {
                    if (w == null) continue;
                    Undo.RecordObject(w, "Bulk Op");
                    op(w);
                    EditorUtility.SetDirty(w);
                }
            }
            else
            {
                Undo.RecordObject(currentWave, "Bulk Op");
                op(currentWave);
                EditorUtility.SetDirty(currentWave);
            }
        }

        // ============================================================
        // Row operations
        // ============================================================

        private void AddRow(WaveAsset wave)
        {
            Undo.RecordObject(wave, "Add SpawnGroup");
            var list = new List<SpawnGroup>(wave.groups ?? new SpawnGroup[0])
            {
                new SpawnGroup { count = 1, interval = 0.5f, startTime = 0f },
            };
            wave.groups = list.ToArray();
            EditorUtility.SetDirty(wave);
        }

        private void RemoveRow(WaveAsset wave, int index)
        {
            if (wave.groups == null || index < 0 || index >= wave.groups.Length) return;
            Undo.RecordObject(wave, "Remove SpawnGroup");
            var list = new List<SpawnGroup>(wave.groups);
            list.RemoveAt(index);
            wave.groups = list.ToArray();
            EditorUtility.SetDirty(wave);
        }

        private void DuplicateRow(WaveAsset wave, int index)
        {
            if (wave.groups == null || index < 0 || index >= wave.groups.Length) return;
            Undo.RecordObject(wave, "Duplicate SpawnGroup");
            var list = new List<SpawnGroup>(wave.groups);
            var src = list[index];
            list.Insert(index + 1, new SpawnGroup
            {
                enemyPrefab = src.enemyPrefab,
                route       = src.route,
                startTime   = src.startTime,
                count       = src.count,
                interval    = src.interval,
            });
            wave.groups = list.ToArray();
            EditorUtility.SetDirty(wave);
        }

        private void MoveRow(WaveAsset wave, int index, int dir)
        {
            if (wave.groups == null) return;
            int target = index + dir;
            if (index < 0 || index >= wave.groups.Length) return;
            if (target < 0 || target >= wave.groups.Length) return;

            Undo.RecordObject(wave, "Move SpawnGroup");
            (wave.groups[index], wave.groups[target]) = (wave.groups[target], wave.groups[index]);
            EditorUtility.SetDirty(wave);
        }

        // ============================================================
        // Wave operations
        // ============================================================

        private void AddWave()
        {
            var folder = ResolveWavesFolder();
            var wave = ScriptableObject.CreateInstance<WaveAsset>();
            wave.waveName = $"Wave_{((level.waves?.Length ?? 0) + 1):00}";
            wave.delayBeforeNextWave = 2f;
            wave.groups = new SpawnGroup[0];

            var path = AssetDatabase.GenerateUniqueAssetPath(
                $"{folder}/{SanitizeFileName(wave.waveName)}.asset");
            AssetDatabase.CreateAsset(wave, path);
            Undo.RegisterCreatedObjectUndo(wave, "Add Wave");

            Undo.RecordObject(level, "Add Wave");
            var arr = level.waves ?? new WaveAsset[0];
            System.Array.Resize(ref arr, arr.Length + 1);
            arr[arr.Length - 1] = wave;
            level.waves = arr;
            EditorUtility.SetDirty(level);
            AssetDatabase.SaveAssets();

            selectedWaveIndex = arr.Length - 1;
        }

        private void DuplicateCurrentWave()
        {
            if (level.waves == null || selectedWaveIndex < 0 || selectedWaveIndex >= level.waves.Length) return;
            var src = level.waves[selectedWaveIndex];
            if (src == null) return;

            var folder = ResolveWavesFolder();
            var copy = Instantiate(src);
            copy.waveName = src.waveName + "_copy";
            // groups は MemberwiseClone 的な参照コピーなので、各 SpawnGroup を deep copy しておく
            if (src.groups != null)
            {
                copy.groups = new SpawnGroup[src.groups.Length];
                for (int i = 0; i < src.groups.Length; i++)
                {
                    var s = src.groups[i];
                    if (s == null) continue;
                    copy.groups[i] = new SpawnGroup
                    {
                        enemyPrefab = s.enemyPrefab,
                        route       = s.route,
                        startTime   = s.startTime,
                        count       = s.count,
                        interval    = s.interval,
                    };
                }
            }

            var path = AssetDatabase.GenerateUniqueAssetPath(
                $"{folder}/{SanitizeFileName(copy.waveName)}.asset");
            AssetDatabase.CreateAsset(copy, path);
            Undo.RegisterCreatedObjectUndo(copy, "Duplicate Wave");

            Undo.RecordObject(level, "Duplicate Wave");
            var list = new List<WaveAsset>(level.waves);
            list.Insert(selectedWaveIndex + 1, copy);
            level.waves = list.ToArray();
            EditorUtility.SetDirty(level);
            AssetDatabase.SaveAssets();

            selectedWaveIndex++;
        }

        private void DeleteCurrentWave()
        {
            if (level.waves == null || selectedWaveIndex < 0 || selectedWaveIndex >= level.waves.Length) return;

            var ok = EditorUtility.DisplayDialog(
                "Delete Wave",
                $"Wave {selectedWaveIndex} を Level から外しますか？\n（.asset ファイルそのものは削除しません。)",
                "削除", "キャンセル");
            if (!ok) return;

            Undo.RecordObject(level, "Delete Wave");
            var list = new List<WaveAsset>(level.waves);
            list.RemoveAt(selectedWaveIndex);
            level.waves = list.ToArray();
            EditorUtility.SetDirty(level);

            if (selectedWaveIndex >= level.waves.Length)
            {
                selectedWaveIndex = Mathf.Max(0, level.waves.Length - 1);
            }
        }

        private void MoveCurrentWave(int dir)
        {
            if (level.waves == null) return;
            int idx = selectedWaveIndex;
            int target = idx + dir;
            if (idx < 0 || idx >= level.waves.Length) return;
            if (target < 0 || target >= level.waves.Length) return;

            Undo.RecordObject(level, "Move Wave");
            (level.waves[idx], level.waves[target]) = (level.waves[target], level.waves[idx]);
            EditorUtility.SetDirty(level);
            selectedWaveIndex = target;
        }

        // ============================================================
        // Utilities
        // ============================================================

        /// <summary>
        /// 新しい WaveAsset の保存先フォルダを決める。
        /// 1) 既存の WaveAsset があればそのフォルダ
        /// 2) 無ければ LevelAsset と同じ階層に "Waves" を作る
        /// 3) 最終的に "Assets" にフォールバック
        /// </summary>
        private string ResolveWavesFolder()
        {
            if (level.waves != null)
            {
                foreach (var w in level.waves)
                {
                    if (w == null) continue;
                    var p = AssetDatabase.GetAssetPath(w);
                    if (string.IsNullOrEmpty(p)) continue;
                    return Path.GetDirectoryName(p).Replace("\\", "/");
                }
            }

            var levelPath = AssetDatabase.GetAssetPath(level);
            if (!string.IsNullOrEmpty(levelPath))
            {
                var levelFolder = Path.GetDirectoryName(levelPath).Replace("\\", "/");
                var wavesFolder = $"{levelFolder}/Waves";
                if (!AssetDatabase.IsValidFolder(wavesFolder))
                {
                    AssetDatabase.CreateFolder(levelFolder, "Waves");
                }
                return wavesFolder;
            }

            return "Assets";
        }

        private static string SanitizeFileName(string name)
        {
            foreach (var c in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(c, '_');
            }
            return name;
        }
    }
}
