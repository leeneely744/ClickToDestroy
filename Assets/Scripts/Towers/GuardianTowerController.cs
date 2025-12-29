using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ガーディアンタワー共通のコントローラー。
/// レベルごとの差分（初期レベル、次レベルプレハブ、ガーディアンのプレハブ数・種類、タワーの高さ調整など）は
/// インスペクタから設定できるようにし、スクリプト自体は1つにまとめる。
/// </summary>
public class GuardianTowerController : GuardianTowerControllerBase
{
    [Header("Level")]
    [SerializeField] private int initialLevelIndex = 0;
    [SerializeField] private GameObject nextLevelPrefab;

    [Header("Guardian")]
    [SerializeField] private GameObject guardianPrefab;

    [Header("Transform")]
    [Tooltip("1 以外の場合、Start 時に Y 座標に倍率を掛けて高さを調整します（例: 1.1f で 10% 高く）。")]
    [SerializeField] private float heightScaleY = 1f;

    protected override int InitialLevelIndex => initialLevelIndex;
    public override GameObject NextLevelPrefab => nextLevelPrefab;
    protected override GameObject GuardianPrefab => guardianPrefab;

    protected override void Start()
    {
        base.Start();

        // レベル3相当など、背の高いガーディアンタワー用に高さを調整
        if (!Mathf.Approximately(heightScaleY, 1f))
        {
            var pos = transform.position;
            pos.y *= heightScaleY;
            transform.position = pos;
        }
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
