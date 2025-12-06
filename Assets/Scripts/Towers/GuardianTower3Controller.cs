using UnityEngine;

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
}
