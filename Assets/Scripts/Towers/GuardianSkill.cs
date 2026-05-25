using UnityEngine;

public abstract class GuardianSkill : MonoBehaviour
{
    public virtual void OnAttack(EnemyController target, int attackDamage) { }
    public virtual void OnKill(EnemyController killed) { }
}
