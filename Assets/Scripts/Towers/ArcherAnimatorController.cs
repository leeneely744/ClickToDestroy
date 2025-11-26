using UnityEngine;

public class ArcherAnimatorController : MonoBehaviour
{
    private Animator animator;

    [SerializeField] private string attackTriggerName = "AttackTrigger";

    private void Awake()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator component not found on Archer.");
        }
    }

    public void PlayAttack()
    {
        if (animator == null)
        {
            return;
        }

        animator.SetTrigger(attackTriggerName);
    }
}
