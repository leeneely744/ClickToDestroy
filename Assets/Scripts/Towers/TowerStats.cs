using UnityEngine;

[CreateAssetMenu(menuName = "TowerStats", fileName = "TowerStats")]
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
    public string towerName;
    public GameObject nextLevelPrefab;

    [Header("Numbers")]
    public int cost;
    public int sellRefund;
    public float attackInterval;
    public float range;

    [Header("Flags")]
    public bool isFusionTower;
    public bool canBeFusionMaterial;
    public bool hasLevel;

    // スキル
    // public SkillDefinition[] skills;
}
