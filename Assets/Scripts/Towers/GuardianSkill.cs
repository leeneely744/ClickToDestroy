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

    // true を返すとダメージを無効化する（忍者の回避など）
    public virtual bool OnTakeDamage(int damage) => false;

    public virtual void OnDeath() { }
}
