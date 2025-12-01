using UnityEngine;

public class GuardianController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float stopDistance = 0.05f;
    private int hp = 100;
    private bool hasMoveTarget;
    private Vector3 moveTarget;
    private bool isDead;
    private GuardianTowerControllerBase ownerTower;

    void Awake()
    {
        ownerTower = GetComponentInParent<GuardianTowerControllerBase>();
    }

    void Update()
    {
        HandleMovement();
        HandleDeath();
    }

    public void SetMoveTarget(Vector3 targetPosition)
    {
        moveTarget = targetPosition;
        hasMoveTarget = true;
    }

    private void HandleMovement()
    {
        if (!hasMoveTarget)
        {
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, moveTarget, moveSpeed * Time.deltaTime);
        if (Vector3.Distance(transform.position, moveTarget) <= stopDistance)
        {
            hasMoveTarget = false;
        }
    }

    private void HandleDeath()
    {
        if (isDead || hp > 0)
        {
            return;
        }

        isDead = true;
        if (ownerTower != null)
        {
            ownerTower.OnGuardianDestroyed(ownerTower.AttackInterval);
        }

        Destroy(gameObject);
    }
}
