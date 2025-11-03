using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public int appearanceLimit = 10;
    public int appearanceCount = 0;

    public GameObject[] enemys;

    public Route route;

    private float spawnInterval = 2.0f;

    void Start()
    {
        if (enemys.Length == 0)
        {
            return;
        }

        InvokeRepeating(
            nameof(SpawnEnemy),
            0.0f,
            spawnInterval);
    }

    void SpawnEnemy()
    {
        if (appearanceCount >= appearanceLimit)
        {
            return;
        }

        var enemy = Instantiate(
            enemys[Random.Range(0, enemys.Length)],
            transform.position,
            Quaternion.identity
        );

        enemy.GetComponent<EnemyController>().SetRoute(route);
        
        appearanceCount++;
    }
}
