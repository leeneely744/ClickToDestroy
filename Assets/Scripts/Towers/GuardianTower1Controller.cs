using UnityEngine;

public class GuardianTower1Controller : GuardianTowerControllerBase
{
    [SerializeField] private GameObject nextLevelPrefab;
    [SerializeField] private int maxSoldiers = 3;
    [SerializeField] private GameObject guardianPrefab;

    protected override int InitialLevelIndex => 0;
    public override GameObject NextLevelPrefab => nextLevelPrefab;
    public override int GetMaxUnits() => maxSoldiers;

    protected override GameObject GuardianPrefab => guardianPrefab;

    protected override void SpawnGuardians()
    {
        int guardiansToSpawn = maxSoldiers - currentGuardianCount;

        if (GuardianPrefab == null)
        {
            Debug.LogWarning($"Guardian prefab is not set on {name}.");
            return;
        }

        for (int i = 0; i < guardiansToSpawn; i++)
        {
            // ガーディアンの生成位置をランダムに設定
            Vector3 spawnPosition = transform.position + new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
            GameObject guardian = Instantiate(GuardianPrefab, spawnPosition, Quaternion.identity, transform);
            string guardianName = guardianNames[currentGuardianCount % guardianNames.Length];
            guardian.name = guardianName;
            currentGuardianCount++;
        }
    }
}
