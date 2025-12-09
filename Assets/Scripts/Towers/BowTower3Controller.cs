public class BowTower3Controller : TowerController
{
    protected override int InitialLevelIndex => 2;

    protected override void Start()
    {
        base.Start();
        // 他よりも背の高いタワーなので、立ち位置を調整する
        var pos = transform.position;
        pos.y *= 1.4f;
        transform.position = pos;
    }
}
