using System.Collections;
using UnityEngine;
using AnimatorParams = Constants.AnimatorParams;

public class FireElementRampageSkill : TowerSkill
{
    [SerializeField] private float rampageInterval = 0.1f;
    [SerializeField] private float rampageDuration = 2f;
    [SerializeField] private float cooldown = 20f;

    private bool isRampaging;
    private bool isOnCooldown;
    private Animator animator;

    protected override void Awake()
    {
        base.Awake();
        animator = GetComponent<Animator>();
    }

    public override void OnAttack(EnemyController target, int attackDamage)
    {
        if (!IsPurchased || isRampaging || isOnCooldown) return;
        StartCoroutine(Rampage());
    }

    private IEnumerator Rampage()
    {
        isRampaging = true;
        Owner.SetAttackIntervalOverride(rampageInterval);
        if (animator != null) animator.SetBool(AnimatorParams.IsSkillActive, true);

        yield return new WaitForSeconds(rampageDuration);

        Owner.ClearAttackIntervalOverride();
        if (animator != null) animator.SetBool(AnimatorParams.IsSkillActive, false);
        isRampaging = false;

        isOnCooldown = true;
        yield return new WaitForSeconds(cooldown);
        isOnCooldown = false;
    }
}
