using System.Collections;
using System.Collections.Generic;
using TD.Spawning;
using UnityEngine;
using UnityEngine.Serialization;

public class EnemySpawner : MonoBehaviour
{
    // ====== 新フィールド ======
    [Header("Level")]
    [SerializeField] private LevelAsset level;

    [Header("Refs")]
    [SerializeField] private ScoreBoard scoreBoard;

    // ====== Legacy フィールド（移行用に残してある。移行が完了したら削除可） ======
    // 旧フィールド名 ("waves", "routes", "startDelay") から FormerlySerializedAs で
    // データを引き継ぐ。エディタ上でのみ移行ツールから読み出す。
    [System.Serializable]
    public class SpawnInstruction
    {
        public GameObject enemyPrefab;
        public int routeIndex = 0;
        public float timeFromWaveStart = 0.5f;
    }

    [System.Serializable]
    public class WaveDefinition
    {
        public string waveName = "Wave";
        public SpawnInstruction[] spawns;
        public float delayBeforeNextWave = 2f;
    }

    [Header("Legacy (to be migrated)")]
    [FormerlySerializedAs("waves")]
    [SerializeField] private WaveDefinition[] legacyWaves;

    [FormerlySerializedAs("routes")]
    [SerializeField] private Route[] legacyRoutes;

    [FormerlySerializedAs("startDelay")]
    [SerializeField] private float legacyStartDelay = 1f;
    // =================================================================

    private int activeEnemies = 0;

    private void Awake()
    {
        if (scoreBoard == null)
        {
            scoreBoard = FindAnyObjectByType<ScoreBoard>();
        }

        if (scoreBoard == null)
        {
            Debug.LogError("EnemySpawner: ScoreBoard がシーンに存在しません。", this);
            enabled = false;
        }
    }

    private void Start()
    {
        if (!enabled)
        {
            return;
        }

        if (level == null || level.waves == null || level.waves.Length == 0)
        {
            Debug.LogWarning(
                "EnemySpawner: LevelAsset が割り当てられていません。" +
                "Tools > TD > Migrate Selected EnemySpawner to LevelAsset から移行してください。",
                this);
            return;
        }

        StartCoroutine(RunWaves());
    }

    private IEnumerator RunWaves()
    {
        yield return new WaitForSeconds(level.startDelay);

        for (int waveIndex = 0; waveIndex < level.waves.Length; waveIndex++)
        {
            var wave = level.waves[waveIndex];
            if (wave == null)
            {
                continue;
            }

            yield return StartCoroutine(SpawnWave(wave));

            // 敵が全滅するまで待つ
            while (activeEnemies > 0)
            {
                yield return null;
            }

            yield return new WaitForSeconds(wave.delayBeforeNextWave);
        }

        while (activeEnemies > 0)
        {
            yield return null;
        }

        if (scoreBoard.CurrentHp > 0)
        {
            GameManager.Instance?.HandleGameClear();
        }
    }

    private IEnumerator SpawnWave(WaveAsset wave)
    {
        if (wave.groups == null || wave.groups.Length == 0)
        {
            yield break;
        }

        // SpawnGroup を 1 体ずつのスケジュールに展開してから時刻順にソートする。
        var schedule = new List<(float time, GameObject prefab, Route route)>();
        foreach (var g in wave.groups)
        {
            if (g.enemyPrefab == null || g.route == null)
            {
                continue;
            }

            int count = Mathf.Max(1, g.count);
            float interval = Mathf.Max(0f, g.interval);
            for (int i = 0; i < count; i++)
            {
                schedule.Add((g.startTime + interval * i, g.enemyPrefab, g.route));
            }
        }

        schedule.Sort((a, b) => a.time.CompareTo(b.time));

        float currentTime = 0f;
        foreach (var s in schedule)
        {
            float wait = s.time - currentTime;
            if (wait > 0f)
            {
                yield return new WaitForSeconds(wait);
                currentTime += wait;
            }

            SpawnEnemy(s.prefab, s.route);
        }
    }

    private void SpawnEnemy(GameObject prefab, Route route)
    {
        var enemy = Instantiate(prefab, transform.position, Quaternion.identity);
        var controller = enemy.GetComponent<EnemyController>();
        if (controller != null)
        {
            controller.SetRoute(route);
            controller.SetSpawner(this);
        }

        activeEnemies++;
    }

    public void NotifyEnemyRemoved(EnemyController enemy)
    {
        activeEnemies = Mathf.Max(0, activeEnemies - 1);
    }

#if UNITY_EDITOR
    // ===== 移行ツール専用のアクセサ =====
    public WaveDefinition[] GetLegacyWaves() => legacyWaves;
    public Route[] GetLegacyRoutes() => legacyRoutes;
    public float GetLegacyStartDelay() => legacyStartDelay;
    public void AssignLevel(LevelAsset asset) { level = asset; }
#endif
}
