using UnityEngine;

[RequireComponent(typeof(TowerAttackController))]
public abstract class TowerSkill : MonoBehaviour, IPurchasableSkill
{
    [SerializeField] private string skillName = "";
    [SerializeField] private int cost = 0;
    private bool isPurchased = false;

    public string SkillName => skillName;
    public int Cost => cost;
    public bool IsPurchased => isPurchased;

    protected TowerAttackController Owner { get; private set; }

    protected virtual void Awake()
    {
        Owner = GetComponent<TowerAttackController>();
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

    // バフ型（常時発動）: Configure 完了後に一度だけ呼ばれる。stats の書き換えや初期化に使う
    public virtual void OnInitialize(TowerAttackController attackController) { }

    // 技型（確率発動）: 攻撃が命中するたびに呼ばれる
    public virtual void OnAttack(EnemyController target, int attackDamage) { }

    // 購入時に一度だけ呼ばれる。派生クラスでバフ効果などを実装する
    protected virtual void OnActivate() { }
}
