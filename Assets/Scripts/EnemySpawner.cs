using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public int appearanceLimit = 10;
    public int appearanceCount = 0;

    public GameObject[] enemys;

    private float spawnInterval = 2.0f;

    void Start()
    {
        startTime = Time.time;
        InvokeRepeating(
            nameof(SpawnEnemy),
            0.0f,
            spawnInterval);
    }

    void SpawnEnemy()
    {
        Debug.Log("SpawnEnemy");
        // var randomIndex = Random.Range(0, enemys.Length);
        // var newEnemy = Instantiate(enemys[randomIndex], transform.position, Quaternion.identity);
        // newEnemy.transform.position = transform.position;
    }
}
