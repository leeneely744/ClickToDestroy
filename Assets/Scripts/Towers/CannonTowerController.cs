using UnityEngine;

public class CannonTowerController : TowerController
{
    [SerializeField] private GameObject nextLevelPrefab;
    [SerializeField] private string attackTriggerName = "isAttack";

    protected override int InitialLevelIndex => 0;
    public override GameObject NextLevelPrefab => nextLevelPrefab;

    /// <summary>
    /// CannonTower では、タワー本体に付いている Animator の攻撃モーションを発火させる。
    /// Bow/Magic のように子オブジェクトの人型キャラを動かすのではなく、本体スプライトをアニメーションさせる想定。
    /// </summary>
    protected override void Attack(EnemyController target)
    {
        base.Attack(target);

        var animator = GetComponent<Animator>();
        if (animator == null)
        {
            return;
        }

        animator.SetTrigger(attackTriggerName);
    }
}
