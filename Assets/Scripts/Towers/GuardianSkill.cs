using UnityEngine;

[RequireComponent(typeof(GuardianController))]
public abstract class GuardianSkill : MonoBehaviour
{
    protected GuardianController Owner { get; private set; }

    protected virtual void Awake()
    {
        Owner = GetComponent<GuardianController>();
    }

    public virtual void OnAttack(EnemyController target, int attackDamage) { }
    public virtual void OnKill(EnemyController killed) { }
}
