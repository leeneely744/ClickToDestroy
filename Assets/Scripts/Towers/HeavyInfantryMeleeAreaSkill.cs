using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class HeavyInfantryMeleeAreaSkill : GuardianSkill
{
    // 近接範囲攻撃は購入不要の常時発動パッシブ
    public override bool IsPurchased => true;

    [SerializeField] private float areaRadius = 0.8f;
    [SerializeField, Range(0f, 1f)] private float damageRatio = 1f;

    public override void OnAttack(EnemyController target, int attackDamage)
    {
        int skillDamage = Mathf.RoundToInt(attackDamage * damageRatio);
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, areaRadius);
        foreach (var col in hits)
        {
            var enemy = col.GetComponent<EnemyController>();
            if (enemy == null || enemy.IsDead || enemy.IsFlying)
                continue;
            if (enemy == target)
                // メインターゲットはすでにGuardianControllerで攻撃しており、
                // ここで再度ダメージを与えると二重攻撃になってしまうため、
                // スキルのダメージは与えない。
                continue;

            enemy.TakeDamage(skillDamage, AttackType.Physical);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, areaRadius);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(HeavyInfantryMeleeAreaSkill))]
public class HeavyInfantryMeleeAreaSkillEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("areaRadius"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("damageRatio"));
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
