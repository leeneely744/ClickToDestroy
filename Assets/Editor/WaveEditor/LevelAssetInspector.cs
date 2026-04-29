using TD.Spawning;
using UnityEditor;
using UnityEngine;

namespace TD.SpawningEditor
{
    [CustomEditor(typeof(LevelAsset))]
    public class LevelAssetInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Open in Wave Editor", GUILayout.Height(28)))
            {
                WaveEditorWindow.OpenWith((LevelAsset)target);
            }
            EditorGUILayout.Space();
            DrawDefaultInspector();
        }
    }
}
