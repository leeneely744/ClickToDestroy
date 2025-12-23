using UnityEngine;

[CreateAssetMenu(menuName = "Tower/TowerStats", fileName = "TowerStats")]
public class TowerStats : ScriptableObject
{
    public TowerLevel[] levels;
}

[System.Serializable]
public class TowerLevel
{
    // TowerDefinition から移管した ID / 表示名 / 見た目など

    [Header("Identity")]
    public string towerId;
    public string displayName;

    [Header("Visuals")]
    public TowerAnimationMode animationMode;
    [Tooltip("animationMode が ChildAnimator の場合に参照する子オブジェクトのパス/名前")]
    public string childAnimatorPath;
    [Tooltip("攻撃時に Animator に送る Trigger 名")]
    public string attackTriggerName;
    public AnimatorOverrideController animatorOverride;
    public Sprite icon;

    [Header("Numbers")]
    public string towerName;
    public int cost;
    public int sellRefund;
    public float attackDamage;
    public float attackInterval;
    public float range;

    [Header("Flags")]
    public bool isFusionTower;
    public bool canBeFusionMaterial;
    public bool hasLevel;

    [Header("Guardian")]
    public GameObject guardianPrefab;

    // スキル
    // public SkillDefinition[] skills;
}
