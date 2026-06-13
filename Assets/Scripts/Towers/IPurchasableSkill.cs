public interface IPurchasableSkill
{
    int Cost { get; }
    bool IsPurchased { get; }

    // お金を消費してスキルを有効化する。お金が足りない・購入済みの場合は false を返す
    bool TryPurchase(Money money);

    // お金を消費せずにスキルを有効化する（購入済みの伝播用）
    void Activate();
}
