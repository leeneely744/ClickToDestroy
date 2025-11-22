using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [System.Serializable]
    public class SpawnInstruction
    {
        public GameObject enemyPrefab;
        public int count = 1;
        public float interval = 0.5f;
        public int routeIndex = 0;
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
            scoreBoard = FindObjectOfType<ScoreBoard>();
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

        foreach (var instruction in wave.spawns)
        {
            if (instruction.enemyPrefab == null)
            {
                continue;
            }

            for (int i = 0; i < instruction.count; i++)
            {
                SpawnEnemy(instruction.enemyPrefab, routes[instruction.routeIndex]);
                yield return new WaitForSeconds(Mathf.Max(0f, instruction.interval));
            }
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
