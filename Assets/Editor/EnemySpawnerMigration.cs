using System.IO;
using TD.Spawning;
using UnityEditor;
using UnityEngine;

/// <summary>
/// シーン上の Route コンポーネントに対して RouteAsset を一括で生成・バインドする
/// エディタユーティリティ。新しく Route を追加したときに使う。
/// </summary>
public static class EnemySpawnerMigration
{
    [MenuItem("Tools/TD/Bind Scene Routes to RouteAssets")]
    public static void BindRoutes()
    {
        BindRoutesInternal(forceRebind: false);
    }

    /// <summary>
    /// シーン上の全 Route について、既存のバインドを上書きして
    /// GameObject 名ごとに新しい RouteAsset を1個ずつ作り直す。
    ///
    /// 用途: 同じ RouteAsset が複数 Route に割り当たってしまっているのを
    /// 1:1 に正すとき（例: 1本の道に複数レーンを増やしたあと）。
    ///
    /// 注意: 古い RouteAsset 自体は削除されない。WaveAsset から古い
    /// アセットを参照している場合は、別途手動で新アセットに張り替える必要がある。
    /// </summary>
    [MenuItem("Tools/TD/Rebind All Scene Routes (Force, 1 Asset per Route)")]
    public static void RebindRoutes()
    {
        BindRoutesInternal(forceRebind: true);
    }

    private static void BindRoutesInternal(bool forceRebind)
    {
        var routes = Object.FindObjectsByType<Route>(FindObjectsSortMode.InstanceID);
        if (routes.Length == 0)
        {
            EditorUtility.DisplayDialog("Bind Routes", "シーンに Route コンポーネントが見つかりません。", "OK");
            return;
        }

        if (forceRebind)
        {
            int alreadyBoundCount = 0;
            foreach (var r in routes) if (r.Asset != null) alreadyBoundCount++;
            if (alreadyBoundCount > 0)
            {
                bool ok = EditorUtility.DisplayDialog(
                    "Rebind Routes (Force)",
                    $"既に RouteAsset がバインドされている Route が {alreadyBoundCount} 個あります。\n" +
                    "これらを新しい RouteAsset で上書きしてよろしいですか？\n\n" +
                    "・古い RouteAsset アセット自体は削除されません。\n" +
                    "・WaveAsset から古いアセットを参照している場合は、別途手動で\n" +
                    "  新しいアセットに張り替えてください。",
                    "上書きする", "キャンセル");
                if (!ok) return;
            }
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
            if (!forceRebind && route.Asset != null) { alreadyBound++; continue; }

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
            forceRebind ? "Rebind Routes (Force)" : "Bind Routes",
            forceRebind
                ? $"完了。新規 RouteAsset {created} 個を作成し、すべての Route にバインドしました。\n\n" +
                  "次のステップ:\n" +
                  "・WaveAsset が古い RouteAsset を参照している場合、新しいアセットに張り替える\n" +
                  "・参照されなくなった古い RouteAsset を削除する"
                : $"完了。\n\n新規 RouteAsset: {created}\n既にバインド済み: {alreadyBound}",
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
