using System.Collections.Generic;
using System.IO;
using TD.Spawning;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 旧 EnemySpawner.WaveDefinition[] を、新しい LevelAsset / WaveAsset / RouteAsset に
/// 変換するためのエディタ拡張。
/// </summary>
public static class EnemySpawnerMigration
{
    [MenuItem("Tools/TD/Bind Scene Routes to RouteAssets")]
    public static void BindRoutes()
    {
        var routes = Object.FindObjectsByType<Route>(FindObjectsSortMode.InstanceID);
        if (routes.Length == 0)
        {
            EditorUtility.DisplayDialog("Bind Routes", "シーンに Route コンポーネントが見つかりません。", "OK");
            return;
        }

        var folder = EditorUtility.SaveFolderPanel(
            "RouteAsset の保存先を選択", "Assets", "");
        if (string.IsNullOrEmpty(folder)) return;
        if (!folder.StartsWith(Application.dataPath))
        {
            EditorUtility.DisplayDialog("Bind Routes", "Assets 配下のフォルダを選択してください。", "OK");
            return;
        }
        var relFolder = "Assets" + folder.Substring(Application.dataPath.Length);
        EnsureFolder(relFolder);

        int created = 0;
        int alreadyBound = 0;
        foreach (var route in routes)
        {
            if (route.Asset != null) { alreadyBound++; continue; }

            var asset = ScriptableObject.CreateInstance<RouteAsset>();
            asset.description = $"Auto-generated for scene Route '{route.name}'";
            var fileName = SanitizeFileName($"Route_{route.name}.asset");
            var path = AssetDatabase.GenerateUniqueAssetPath(
                Path.Combine(relFolder, fileName).Replace("\\", "/"));
            AssetDatabase.CreateAsset(asset, path);

            Undo.RecordObject(route, "Assign RouteAsset");
            route.EditorAssignRouteAsset(asset);
            EditorUtility.SetDirty(route);
            created++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();

        EditorUtility.DisplayDialog(
            "Bind Routes",
            $"完了。\n\n新規 RouteAsset: {created}\n既にバインド済み: {alreadyBound}",
            "OK");
    }

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

        var folder = EditorUtility.SaveFolderPanel(
            "LevelAsset / WaveAsset / RouteAsset の保存先を選択", "Assets", "");
        if (string.IsNullOrEmpty(folder)) return;
        if (!folder.StartsWith(Application.dataPath))
        {
            EditorUtility.DisplayDialog("Migration", "Assets 配下のフォルダを選択してください。", "OK");
            return;
        }

        var relFolder = "Assets" + folder.Substring(Application.dataPath.Length);
        EnsureFolder(relFolder);
        var wavesFolder = Path.Combine(relFolder, "Waves").Replace("\\", "/");
        EnsureFolder(wavesFolder);
        var routesFolder = Path.Combine(relFolder, "Routes").Replace("\\", "/");
        EnsureFolder(routesFolder);

        // === 1. 各 legacyRoute に対応する RouteAsset を確保する ===
        var routeAssetMap = new Dictionary<Route, RouteAsset>();
        if (legacyRoutes != null)
        {
            foreach (var route in legacyRoutes)
            {
                if (route == null) continue;
                if (routeAssetMap.ContainsKey(route)) continue;

                RouteAsset routeAsset = route.Asset;
                if (routeAsset == null)
                {
                    routeAsset = ScriptableObject.CreateInstance<RouteAsset>();
                    routeAsset.description = $"Auto-generated for scene Route '{route.name}'";
                    var fileName = SanitizeFileName($"Route_{route.name}.asset");
                    var path = AssetDatabase.GenerateUniqueAssetPath(
                        Path.Combine(routesFolder, fileName).Replace("\\", "/"));
                    AssetDatabase.CreateAsset(routeAsset, path);

                    Undo.RecordObject(route, "Assign RouteAsset");
                    route.EditorAssignRouteAsset(routeAsset);
                    EditorUtility.SetDirty(route);
                }
                routeAssetMap[route] = routeAsset;
            }
        }

        // === 2. WaveAsset / LevelAsset を生成 ===
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
                    RouteAsset routeAsset = null;
                    if (legacyRoutes != null && legacyRoutes.Length > 0 && s.routeIndex >= 0)
                    {
                        var idx = Mathf.Clamp(s.routeIndex, 0, legacyRoutes.Length - 1);
                        var route = legacyRoutes[idx];
                        if (route != null && routeAssetMap.TryGetValue(route, out var ra))
                        {
                            routeAsset = ra;
                        }
                    }

                    wave.groups[j] = new SpawnGroup
                    {
                        enemyPrefab = s.enemyPrefab,
                        route = routeAsset,
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

            var waveFileName = SanitizeFileName($"{i + 1:00}_{wave.waveName}.asset");
            var wavePath = AssetDatabase.GenerateUniqueAssetPath(
                Path.Combine(wavesFolder, waveFileName).Replace("\\", "/"));
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
            "・Level: " + levelPath + "\n" +
            "・WaveAsset 数: " + level.waves.Length + "\n" +
            "・RouteAsset 数: " + routeAssetMap.Count + "\n\n" +
            "Play して挙動を確認してください。",
            "OK");
    }

    private static void EnsureFolder(string assetPath)
    {
        if (AssetDatabase.IsValidFolder(assetPath)) return;

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
