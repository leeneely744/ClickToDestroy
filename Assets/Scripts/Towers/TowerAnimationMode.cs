using UnityEngine;

/// <summary>
/// タワーがどのようにアニメーションするかを表す種別。
/// </summary>
public enum TowerAnimationMode
{
    /// <summary>
    /// アニメーションを持たない、もしくは攻撃時に特別なアニメを再生しないタワー。
    /// </summary>
    None = 0,

    /// <summary>
    /// タワー本体（ルート GameObject）に付いている Animator を使ってアニメーションする。
    /// 例: 砲台タワーが本体ごと揺れる・反動するケースなど。
    /// </summary>
    SelfAnimator = 1,

    /// <summary>
    /// 子オブジェクトに付いている Animator を使ってアニメーションする。
    /// 例: Bow タワー上の弓兵、魔法タワー上の魔法使いなど。
    /// </summary>
    ChildAnimator = 2
}

