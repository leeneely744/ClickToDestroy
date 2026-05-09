using System.Collections;
using System.Collections.Generic;
using TD.Spawning;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Level")]
    [SerializeField] private LevelAsset level;

    [Header("Refs")]
    [SerializeField] private ScoreBoard scoreBoard;

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

        if (level == null)
        {
            Debug.LogWarning(
                "EnemySpawner: LevelAsset が null です。Inspector で level フィールドに LevelAsset を割り当ててください。",
                this);
            return;
        }

        if (level.waves == null || level.waves.Length == 0)
        {
            Debug.LogWarning($"[EnemySpawner] LevelAsset '{level.name}' の waves が空です。LevelAsset に WaveAsset を追加してください。", this);
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
                Debug.LogWarning($"[EnemySpawner] waves[{waveIndex}] が null です。スキップします。", this);
                continue;
            }

            GameManager.Instance?.UpdateWave(waveIndex + 1, level.waves.Length);
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
            Debug.LogWarning($"[EnemySpawner] Wave '{wave.waveName}' の groups が空です。WaveAsset に SpawnGroup を追加してください。", this);
            yield break;
        }

        // SpawnGroup を 1 体ずつのスケジュールに展開してから時刻順にソートする。
        var schedule = new List<(float time, GameObject prefab, Route route)>();
        for (int gi = 0; gi < wave.groups.Length; gi++)
        {
            var g = wave.groups[gi];

            if (g == null)
            {
                Debug.LogWarning($"[EnemySpawner] Wave '{wave.waveName}' の groups[{gi}] が null です → スキップ", this);
                continue;
            }

            if (g.enemyPrefab == null || g.route == null)
            {
                Debug.LogWarning($"[EnemySpawner] Wave '{wave.waveName}' groups[{gi}] の SpawnGroup に null があります: enemyPrefab={g.enemyPrefab?.name ?? "null"}, route={g.route?.name ?? "null"} → スキップ", this);
                continue;
            }

            // RouteAsset → シーン上の Route 実体を解決する。
            var resolvedRoute = RouteRegistry.Resolve(g.route);
            if (resolvedRoute == null)
            {
                Debug.LogWarning(
                    $"[EnemySpawner] Wave '{wave.waveName}' groups[{gi}] の RouteAsset '{g.route.name}' に対応する Route がシーンに存在しません。" +
                    "Route コンポーネントの 'asset' フィールドにこの RouteAsset を割り当ててください。 → スキップ",
                    this);
                continue;
            }

            int count = Mathf.Max(1, g.count);
            float interval = Mathf.Max(0f, g.interval);
            for (int i = 0; i < count; i++)
            {
                schedule.Add((g.startTime + interval * i, g.enemyPrefab, resolvedRoute));
            }
        }

        if (schedule.Count == 0)
        {
            Debug.LogWarning($"[EnemySpawner] Wave '{wave.waveName}' スケジュールが空（全グループ null でスキップされた）。敵が出現しません。", this);
            yield break;
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
        else
        {
            Debug.LogError($"[EnemySpawner] prefab '{prefab.name}' に EnemyController が見つかりません。", this);
        }

        activeEnemies++;
    }

    public void NotifyEnemyRemoved(EnemyController enemy)
    {
        activeEnemies = Mathf.Max(0, activeEnemies - 1);
    }
}
