using UnityEngine;

namespace TD.Spawning
{
    /// <summary>
    /// 1 ウェーブ分のスポーン定義をまとめた ScriptableObject。
    /// シーンから切り離されているため、ステージ間で再利用したり、
    /// アセット単位で git diff を取ることができる。
    /// </summary>
    [CreateAssetMenu(menuName = "TD/Wave", fileName = "Wave")]
    public class WaveAsset : ScriptableObject
    {
        [Tooltip("Inspector 上の表示用の名前。実行時のロジックには影響しない。")]
        public string waveName = "Wave";

        [Tooltip("このウェーブで出現する敵のグループ一覧。")]
        public SpawnGroup[] groups;

        [Tooltip("このウェーブが終了してから次のウェーブが開始するまでの秒数。")]
        public float delayBeforeNextWave = 2f;
    }

    /// <summary>
    /// 1 種類の敵を、指定ルートに、指定タイミングから連続で出現させる定義。
    /// count = 1 の場合は単発出現、count > 1 の場合は interval 秒間隔で連続出現する。
    /// </summary>
    [System.Serializable]
    public class SpawnGroup
    {
        public GameObject enemyPrefab;
        public Route route;

        [Tooltip("ウェーブ開始から最初の 1 体が出現するまでの秒数。")]
        public float startTime = 0f;

        [Tooltip("連続で出現させる体数。1 なら単発出現。")]
        [Min(1)] public int count = 1;

        [Tooltip("count > 1 のときの 1 体ごとの出現間隔（秒）。")]
        [Min(0f)] public float interval = 0.5f;
    }
}
