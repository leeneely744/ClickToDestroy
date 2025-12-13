using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    // どの敵（enemyPrefab）を、
    // どのルート（routeIndex）で、
    // ウェーブ開始から何秒後に（timeFromWaveStart）
    // 出現させるかを定義する。
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

    [SerializeField] private WaveDefinition[] waves;
    [SerializeField] private Route[] routes;
    [SerializeField] private float startDelay = 1f;
    [SerializeField] private ScoreBoard scoreBoard;

    private int activeEnemies = 0;
    private bool isRunning;

    private void Start()
    {
        if (waves == null || waves.Length == 0)
        {
            Debug.LogWarning("EnemySpawner: waves are not configured");
            return;
        }

        StartCoroutine(RunWaves());
    }

    private IEnumerator RunWaves()
    {
        isRunning = true;
        yield return new WaitForSeconds(startDelay);

        for (int waveIndex = 0; waveIndex < waves.Length; waveIndex++)
        {
            var wave = waves[waveIndex];
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

        isRunning = false;
        if (scoreBoard == null)
        {
            scoreBoard = FindAnyObjectByType<ScoreBoard>();
        }

        if (scoreBoard != null && scoreBoard.CurrentHp > 0)
        {
            GameManager.Instance?.HandleGameClear();
        }
    }

    private IEnumerator SpawnWave(WaveDefinition wave)
    {
        if (wave.spawns == null)
        {
            yield break;
        }

        // timeFromWaveStart の昇順に並び替える。
        var instructions = wave.spawns;
        System.Array.Sort(instructions, (a, b) => a.timeFromWaveStart.CompareTo(b.timeFromWaveStart));

        float currentTime = 0f;
        foreach (var instruction in instructions)
        {
            if (instruction.enemyPrefab == null)
            {
                continue;
            }
            
            // 前回スポーンからの待ち時間を計算
            float wait = instruction.timeFromWaveStart - currentTime;
            if (wait > 0f)
            {
                yield return new WaitForSeconds(wait);
                currentTime += wait;
            }

            // routeIndex が範囲外だった場合のガード
            int index = Mathf.Clamp(instruction.routeIndex, 0, routes.Length - 1);
            Route route = routes[index];
            SpawnEnemy(instruction.enemyPrefab, route);
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

    public bool IsWaveRunning()
    {
        return isRunning;
    }
}
