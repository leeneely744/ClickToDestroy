using System.Collections;
using UnityEngine;

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
        if (!isRampaging && !isOnCooldown)
            StartCoroutine(Rampage());
    }

    private IEnumerator Rampage()
    {
        isRampaging = true;
        Owner.SetAttackIntervalOverride(rampageInterval);
        if (animator != null) animator.SetTrigger("SkillTrigger");

        yield return new WaitForSeconds(rampageDuration);

        Owner.ClearAttackIntervalOverride();
        isRampaging = false;

        isOnCooldown = true;
        yield return new WaitForSeconds(cooldown);
        isOnCooldown = false;
    }
}
