using UnityEngine;

[RequireComponent(typeof(GuardianController))]
public abstract class GuardianSkill : MonoBehaviour, IPurchasableSkill
{
    [SerializeField] private string skillName = "";
    [SerializeField] private int cost = 0;
    private bool isPurchased = false;

    public string SkillName => skillName;
    public int Cost => cost;
    public virtual bool IsPurchased => isPurchased;

    protected GuardianController Owner { get; private set; }

    protected virtual void Awake()
    {
        Owner = GetComponent<GuardianController>();
    }

    public bool TryPurchase(Money money)
    {
        if (isPurchased) return false;
        if (!money.SpendMoney(cost)) return false;
        Activate();
        return true;
    }

    public void Activate()
    {
        isPurchased = true;
        OnActivate();
    }

    public virtual void OnAttack(EnemyController target, int attackDamage) { }
    public virtual void OnKill(EnemyController killed) { }

    // true を返すとダメージを無効化する（忍者の回避など）
    public virtual bool OnTakeDamage(int damage) => false;

    public virtual void OnDeath() { }

    // 購入時に一度だけ呼ばれる。派生クラスでバフ効果などを実装する
    protected virtual void OnActivate() { }
}
