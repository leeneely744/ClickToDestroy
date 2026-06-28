using UnityEngine;

public class StatusInfo
{
    public string displayName;
    public Sprite icon;

    // null = 非表示
    public int? maxHp;
    public System.Func<int> getCurrentHp;

    public int? attackDamage;
    public float? physicalResistance;  // 0-1
    public float? magicalResistance;   // 0-1
}
