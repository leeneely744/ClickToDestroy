using System.Collections.Generic;
using UnityEngine;

public class ElfArcherAttackController : TowerAttackController
{
    [SerializeField] private ElfArcherSpriteAnimator[] archers;
    private int nextArcherIndex;

    protected override void PlayAttackAnimation()
    {
        if (archers == null || archers.Length == 0) return;

        var active = new List<ElfArcherSpriteAnimator>();
        foreach (var archer in archers)
        {
            if (archer != null && archer.gameObject.activeSelf)
                active.Add(archer);
        }
        if (active.Count == 0) return;

        nextArcherIndex %= active.Count;
        active[nextArcherIndex].PlayAttack();
        nextArcherIndex = (nextArcherIndex + 1) % active.Count;
    }
}
