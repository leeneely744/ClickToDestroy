using UnityEngine;

public class ElfArcherSpeedUpSkill : TowerSkill
{
    [SerializeField] private GameObject archer3;
    [SerializeField] private float boostedInterval = 0.15f;

    protected override void OnActivate()
    {
        if (archer3 != null) archer3.SetActive(true);
        Owner.SetAttackIntervalOverride(boostedInterval);
    }
}
