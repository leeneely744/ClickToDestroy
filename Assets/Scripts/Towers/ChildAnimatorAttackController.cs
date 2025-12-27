using UnityEngine;

/// <summary>
/// 子オブジェクト上の Animator を使って攻撃アニメーションを再生する TowerAttackController の派生クラス。
/// 弓タワーや魔法タワーのように「土台とは別のキャラクターが動く」タイプ向け。
/// </summary>
public class ChildAnimatorAttackController : TowerAttackController
{
    [SerializeField] private string childAnimatorPath = "";
    [SerializeField] private string attackTriggerName = "AttackTrigger";

    protected override void PlayAttackAnimation()
    {
        Animator animator = null;

        // パス指定があれば優先的に探す
        if (!string.IsNullOrEmpty(childAnimatorPath))
        {
            Transform child = transform.Find(childAnimatorPath);
            if (child != null)
            {
                animator = child.GetComponent<Animator>();
            }
        }

        // パス指定で見つからなければ、子孫から最初の Animator を探す
        if (animator == null)
        {
            var animators = GetComponentsInChildren<Animator>();
            foreach (var a in animators)
            {
                if (a.gameObject != gameObject)
                {
                    animator = a;
                    break;
                }
            }
        }

        if (animator == null || string.IsNullOrEmpty(attackTriggerName))
        {
            return;
        }

        animator.SetTrigger(attackTriggerName);
    }
}

