using UnityEngine;
using System.Collections.Generic;

public class GuardianTower3Controller : GuardianTowerControllerBase
{
    protected override int InitialLevelIndex => 2;

    [SerializeField] private GameObject guardianPrefab;
    protected override GameObject GuardianPrefab => guardianPrefab;

    protected override void Start()
    {
        base.Start();
        // 他よりも背の高いタワーなので、立ち位置を調整する
        var pos = transform.position;
        pos.y *= 1.1f;
        transform.position = pos;
    }

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
