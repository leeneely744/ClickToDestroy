using System.IO;
using TD.Spawning;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 旧 EnemySpawner.WaveDefinition[] を、新しい LevelAsset / WaveAsset に変換するための
/// エディタ拡張。シーン中の EnemySpawner を選択した状態でメニューを実行する。
/// </summary>
public static class EnemySpawnerMigration
{
    private const string DefaultFolder = "Assets/Spawning";

    [MenuItem("Tools/TD/Migrate Selected EnemySpawner to LevelAsset")]
    public static void Migrate()
    {
        var go = Selection.activeGameObject;
        var spawner = go != null ? go.GetComponent<EnemySpawner>() : null;
        if (spawner == null)
        {
            EditorUtility.DisplayDialog(
                "Migration",
                "EnemySpawner を持つ GameObject を Hierarchy で選択してから実行してください。",
                "OK");
            return;
        }

        var legacyWaves = spawner.GetLegacyWaves();
        var legacyRoutes = spawner.GetLegacyRoutes();
        var startDelay = spawner.GetLegacyStartDelay();

        if (legacyWaves == null || legacyWaves.Length == 0)
        {
            EditorUtility.DisplayDialog(
                "Migration",
                "Legacy waves が空です。移行する内容がありません。",
                "OK");
            return;
        }

        // 保存先フォルダ
        var folder = EditorUtility.SaveFolderPanel(
            "LevelAsset / WaveAsset の保存先を選択", "Assets", "");
        if (string.IsNullOrEmpty(folder))
        {
            return;
        }
        if (!folder.StartsWith(Application.dataPath))
        {
            EditorUtility.DisplayDialog(
                "Migration",
                "Assets 配下のフォルダを選択してください。",
                "OK");
            return;
        }

        var relFolder = "Assets" + folder.Substring(Application.dataPath.Length);
        EnsureFolder(relFolder);

        var wavesFolder = Path.Combine(relFolder, "Waves").Replace("\\", "/");
        EnsureFolder(wavesFolder);

        // LevelAsset 本体
        var level = ScriptableObject.CreateInstance<LevelAsset>();
        level.startDelay = startDelay;
        level.waves = new WaveAsset[legacyWaves.Length];

        for (int i = 0; i < legacyWaves.Length; i++)
        {
            var src = legacyWaves[i];
            var wave = ScriptableObject.CreateInstance<WaveAsset>();
            wave.waveName = string.IsNullOrEmpty(src.waveName) ? $"Wave_{i + 1:00}" : src.waveName;
            wave.delayBeforeNextWave = src.delayBeforeNextWave;

            if (src.spawns != null && src.spawns.Length > 0)
            {
                wave.groups = new SpawnGroup[src.spawns.Length];
                for (int j = 0; j < src.spawns.Length; j++)
                {
                    var s = src.spawns[j];
                    Route route = null;
                    if (legacyRoutes != null && legacyRoutes.Length > 0 && s.routeIndex >= 0)
                    {
                        var idx = Mathf.Clamp(s.routeIndex, 0, legacyRoutes.Length - 1);
                        route = legacyRoutes[idx];
                    }

                    wave.groups[j] = new SpawnGroup
                    {
                        enemyPrefab = s.enemyPrefab,
                        route = route,
                        startTime = s.timeFromWaveStart,
                        count = 1,
                        interval = 0.5f,
                    };
                }
            }
            else
            {
                wave.groups = new SpawnGroup[0];
            }

            var fileName = SanitizeFileName($"{i + 1:00}_{wave.waveName}.asset");
            var wavePath = AssetDatabase.GenerateUniqueAssetPath(
                Path.Combine(wavesFolder, fileName).Replace("\\", "/"));
            AssetDatabase.CreateAsset(wave, wavePath);
            level.waves[i] = wave;
        }

        var levelFileName = SanitizeFileName($"Level_{spawner.gameObject.scene.name}.asset");
        if (string.IsNullOrEmpty(levelFileName) || levelFileName == "Level_.asset")
        {
            levelFileName = "Level.asset";
        }
        var levelPath = AssetDatabase.GenerateUniqueAssetPath(
            Path.Combine(relFolder, levelFileName).Replace("\\", "/"));
        AssetDatabase.CreateAsset(level, levelPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // EnemySpawner 側に新しい LevelAsset を割り当てる
        Undo.RecordObject(spawner, "Assign LevelAsset");
        spawner.AssignLevel(level);
        EditorUtility.SetDirty(spawner);
        if (!Application.isPlaying)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(spawner.gameObject.scene);
        }

        Selection.activeObject = level;
        EditorGUIUtility.PingObject(level);

        EditorUtility.DisplayDialog(
            "Migration",
            "移行が完了しました。\n\n" +
            "・新規アセット: " + levelPath + "\n" +
            "・WaveAsset 数: " + level.waves.Length + "\n\n" +
            "Play して挙動を確認したのち、EnemySpawner の Legacy フィールド（waves / routes / startDelay）を空にして問題なければ、" +
            "EnemySpawner.cs から legacy 定義を削除してください。",
            "OK");
    }

    private static void EnsureFolder(string assetPath)
    {
        if (AssetDatabase.IsValidFolder(assetPath))
        {
            return;
        }

        var parent = Path.GetDirectoryName(assetPath).Replace("\\", "/");
        var leaf = Path.GetFileName(assetPath);
        if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
        {
            EnsureFolder(parent);
        }
        AssetDatabase.CreateFolder(parent, leaf);
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
