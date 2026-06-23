using UnityEngine;

public class GravityMageFireSkill : TowerSkill
{
    [SerializeField] private float damageMultiplier = 2f;

    protected override void OnActivate()
    {
        var controller = Owner as GravityMageAttackController;
        if (controller == null) return;
        controller.SetDamageMultiplier(damageMultiplier);
        controller.DoubleRoadShakeStrength();
    }
}
