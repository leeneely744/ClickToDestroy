using UnityEngine;

public enum AttackType
{
    Physical,
    Magic
}

[System.Serializable]
public class AttackStats
{
    public int damage;
    public float attackInterval;
    public AttackType attackType;
    [Tooltip("範囲攻撃かどうか。True の場合、attackRange の半径内の対象すべてにダメージを与える")]
    public bool isAreaAttack;
    [Tooltip("範囲攻撃の攻撃半径（isAreaAttack が True のときのみ有効）")]
    public float attackRange;
}

[CreateAssetMenu(menuName = "TD/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Stats")]
    public int maxHp;
    public float moveSpeed;
    public bool isFlying;
    public int rewardMoney;

    [Header("Defense")]
    [Range(0f, 1f)] public float physicalResistance;
    [Range(0f, 1f)] public float magicalResistance;

    [Header("Melee Attack")]
    [Tooltip("近接攻撃のステータス。攻撃しない敵の場合は damage を 0 にする")]
    public AttackStats meleeAttack;

    [Header("Ranged Attack")]
    [Tooltip("遠距離攻撃のステータス。攻撃しない敵の場合は damage を 0 にする")]
    public AttackStats rangedAttack;

    [Header("Animations")]
    public AnimatorOverrideController animatorOverride;
}
