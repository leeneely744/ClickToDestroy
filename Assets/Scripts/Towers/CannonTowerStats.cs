using UnityEngine;

[CreateAssetMenu(menuName = "Tower/CannonTowerStats", fileName = "CannonTowerStats")]
public class CannonTowerStats : TowerStats
{
    [Tooltip("Animator controllers to use per level. Match the order of levels.")]
    [SerializeField] private RuntimeAnimatorController[] animatorControllers;

    public RuntimeAnimatorController GetAnimatorForLevel(int levelIndex)
    {
        if (animatorControllers == null)
        {
            return null;
        }

        if (levelIndex < 0 || levelIndex >= animatorControllers.Length)
        {
            Debug.LogWarning($"{name}: Invalid level index {levelIndex} for animator controllers.");
            return null;
        }

        return animatorControllers[levelIndex];
    }

    public bool HasAnimatorForLevel(int levelIndex)
    {
        return animatorControllers != null
            && levelIndex >= 0
            && levelIndex < animatorControllers.Length
            && animatorControllers[levelIndex] != null;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (levels == null)
        {
            return;
        }

        if (animatorControllers != null && animatorControllers.Length != levels.Length)
        {
            Debug.LogWarning($"{name}: animatorControllers length should match levels length.");
        }
    }
#endif
}
