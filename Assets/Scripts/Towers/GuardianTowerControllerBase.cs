public class GuardianTowerControllerBase : TowerController
{
    protected int maxUnits;

    public override int GetMaxUnits()
    {
        return maxUnits;
    }
}
