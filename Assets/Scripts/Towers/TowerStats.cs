using UnityEngine;

[CreateAssetMenu(menuName = "Tower/TowerStats", fileName = "TowerStats")]
public class TowerStats : ScriptableObject
{
    public TowerLevel[] levels;
}

[System.Serializable]
public class TowerLevel
{
    public string towerName;
    public Sprite towerSprite;
    public int cost;
    public int sellRefund;
    public float attackDamage;
    public float attackInterval;
    public float range;
}
