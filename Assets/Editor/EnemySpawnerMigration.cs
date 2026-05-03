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
