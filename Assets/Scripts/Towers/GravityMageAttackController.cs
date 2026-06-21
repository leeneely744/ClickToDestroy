using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class GravityMageAttackController : TowerAttackController
{
    [SerializeField] private int damage = 10;
    [SerializeField] private float roadShakeDuration = 0.25f;
    [SerializeField] private float roadShakeStrength = 0.06f;

    private GravityMageSpriteAnimator spriteAnimator;
    private float worldRadius;
    private float damageMultiplier = 1f;

    public void SetDamageMultiplier(float multiplier) => damageMultiplier = multiplier;

    private void Awake()
    {
        spriteAnimator = GetComponent<GravityMageSpriteAnimator>();
    }

    private void Start()
    {
        var arc = transform.Find("AttackRangeCircle");
        if (arc != null)
        {
            var col = arc.GetComponent<CircleCollider2D>();
            if (col != null)
                worldRadius = col.radius * arc.lossyScale.x;
        }
    }

    protected override void Attack(EnemyController target)
    {
        int actualDamage = Mathf.RoundToInt(damage * damageMultiplier);

        var targets = new List<EnemyController>(EnemiesInRange);
        foreach (var enemy in targets)
        {
            if (enemy != null && !enemy.IsDead)
                enemy.TakeDamage(actualDamage, AttackType.Magic);
        }

        ShakeRoadsInRange();
        PlayAttackAnimation();
    }

    protected override void PlayAttackAnimation()
    {
        spriteAnimator?.PlayAttack();
    }

    private void ShakeRoadsInRange()
    {
        var roadsParent = GameObject.Find("Roads");
        if (roadsParent == null) return;

        foreach (Transform road in roadsParent.transform)
        {
            if (Vector2.Distance(road.position, transform.position) <= worldRadius)
                road.DOShakePosition(roadShakeDuration, roadShakeStrength, 15, 90, false, true);
        }
    }
}
