using UnityEngine;

/// <summary>
/// タワー本体に付いている Animator を使って攻撃アニメーションを再生する TowerAttackController の派生クラス。
/// 砲台タワーのように「土台そのものが動く」タイプ向け。
/// </summary>
public class SelfAnimatorAttackController : TowerAttackController
{
    [SerializeField] private string attackTriggerName = "isAttack";

    protected override void PlayAttackAnimation()
    {
        var animator = GetComponent<Animator>();
        if (animator == null || string.IsNullOrEmpty(attackTriggerName))
        {
            return;
        }

        animator.SetTrigger(attackTriggerName);
    }
}

