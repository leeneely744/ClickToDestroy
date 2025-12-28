using UnityEngine;

/// <summary>
/// 子オブジェクト上の Animator を使って攻撃アニメーションを再生する TowerAttackController の派生クラス。
/// 弓タワーや魔法タワーのように「土台とは別のキャラクターが動く」タイプ向け。
/// どの子を使うかは自動で判定し、自身以外の子孫にある最初の Animator を利用する。
/// </summary>
public class ChildAnimatorAttackController : TowerAttackController
{
    [SerializeField] private string attackTriggerName = "AttackTrigger";

    protected override void PlayAttackAnimation()
    {
        Animator animator = null;

        // 自身ではなく、子孫にある最初の Animator を探す
        var animators = GetComponentsInChildren<Animator>();
        foreach (var a in animators)
        {
            if (a.gameObject != gameObject)
            {
                animator = a;
                break;
            }
        }

        if (animator == null || string.IsNullOrEmpty(attackTriggerName))
        {
            return;
        }

        animator.SetTrigger(attackTriggerName);
    }
}
