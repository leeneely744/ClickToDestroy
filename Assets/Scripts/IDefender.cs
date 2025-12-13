public interface IDefender
{
    /// <summary>
    /// 指定されたダメージを受ける。
    /// </summary>
    /// <param name="damage">与えられるダメージ量。</param>
    void TakeDamage(int damage);

    /// <summary>
    /// この防衛ユニットがすでに死亡しているかどうか。
    /// </summary>
    bool IsDead { get; }
}

