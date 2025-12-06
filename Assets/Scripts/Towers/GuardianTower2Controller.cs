using UnityEngine;
using System.Collections.Generic;

public class GuardianTower2Controller : GuardianTowerControllerBase
{
    [SerializeField] private GameObject nextLevelPrefab;

    protected override int InitialLevelIndex => 1;
    public override GameObject NextLevelPrefab => nextLevelPrefab;

    [SerializeField] private GameObject guardianPrefab;
    protected override GameObject GuardianPrefab => guardianPrefab;

    protected override void SpawnGuardians()
    {
        if (GuardianPrefab == null)
        {
            Debug.LogWarning($"Guardian prefab is not set on {name}.");
            return;
        }

        GuardianController[] guardians = GetComponentsInChildren<GuardianController>();
        int guardiansToSpawn = Mathf.Max(0, MaxSoldiers - guardians.Length);

        for (int i = 0; i < guardiansToSpawn; i++)
        {
            GameObject guardian = Instantiate(GuardianPrefab, transform.position, Quaternion.identity, transform);
            guardian.name = guardianNames[(guardians.Length + i) % guardianNames.Length];
        }

        // 最新の兵士リストを取得し直す
        guardians = GetComponentsInChildren<GuardianController>();

        List<Vector3> initialPositions = BuildInitialGuardianPositions();
        if (initialPositions.Count > 0)
        {
            MoveGuardians(guardians, initialPositions);
        }
    }
}
