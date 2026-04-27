using UnityEngine;

namespace TD.Spawning
{
    /// <summary>
    /// 1 ステージ分のウェーブ列を表す ScriptableObject。
    /// EnemySpawner はこのアセットを 1 つ参照するだけでよい。
    /// </summary>
    [CreateAssetMenu(menuName = "TD/Level", fileName = "Level")]
    public class LevelAsset : ScriptableObject
    {
        [Tooltip("ゲーム開始から最初のウェーブが始まるまでの秒数。")]
        public float startDelay = 1f;

        [Tooltip("実行順に並べたウェーブ。")]
        public WaveAsset[] waves;
    }
}
