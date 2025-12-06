using UnityEngine;

[CreateAssetMenu(menuName = "Tower/CannonStats", fileName = "CannonTowerStats")]
public class CannonTowerStats : TowerStats
{
    public CannonLevelData[] visuals;
}

[System.Serializable]
public class CannonLevelData
{
    public GameObject visualPrefab;
    public RuntimeAnimatorController animatorController;
}
