using UnityEngine;

[RequireComponent(typeof(TowerAttackController))]
public abstract class TowerSkill : MonoBehaviour
{
    protected TowerAttackController Owner { get; private set; }

    protected virtual void Awake()
    {
        Owner = GetComponent<TowerAttackController>();
    }

    // バフ型（常時発動）: Configure 完了後に一度だけ呼ばれる。stats の書き換えや初期化に使う
    public virtual void OnInitialize(TowerAttackController attackController) { }

    // 技型（確率発動）: 攻撃が命中するたびに呼ばれる
    public virtual void OnAttack(EnemyController target, int attackDamage) { }
}
