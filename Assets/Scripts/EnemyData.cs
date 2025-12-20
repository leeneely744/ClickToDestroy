using UnityEngine;

[CreateAssetMenu(menuName = "TD/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Stats")]
    public int maxHp;
    public int attackPower;
    public float moveSpeed;
    public bool isFlying;
    public int rewardMoney;
    public float attackInterval;

    [Header("Animations")]
    public AnimatorOverrideController animatorOverride;
}

