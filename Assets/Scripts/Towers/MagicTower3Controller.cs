using UnityEngine;

public class MagicTower3Controller : TowerController
{
    protected override int InitialLevelIndex => 2;

    private void Start()
    {
        // 他よりも背の高いタワーなので、立ち位置を調整する
        var pos = transform.position;
        pos.y *= 1.3f;
        transform.position = pos;
    }
}
