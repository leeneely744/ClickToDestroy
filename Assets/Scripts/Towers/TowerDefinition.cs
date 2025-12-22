[CreateAssetMenu(menuName = "Tower/TowerDefinition")]
public class TowerDefinition : ScriptableObject
{
    public string towerId;
    public string displayName;

    // 見た目
    public RuntimeAnimatorController animator;
    public AnimatorOverrideController animatorOverride;
    public Sprite icon;

    // 数値
    public TowerStats stats;

    // 機能フラグ
    public bool isFusionTower;
    public bool canBeFusionMaterial;
    public bool hasLevel;

    // スキル
    public SkillDefinition[] skills;

    // Guardian系など
    public GameObject guardianPrefab;
}
