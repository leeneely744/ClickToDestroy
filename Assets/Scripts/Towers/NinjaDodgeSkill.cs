using UnityEngine;

public class NinjaDodgeSkill : GuardianSkill
{
    [SerializeField, Range(0f, 1f)] private float dodgeChance = 0.3f;

    public override bool OnTakeDamage(int damage)
    {
        if (!IsPurchased) return false;
        return Random.value < dodgeChance;
    }
}
