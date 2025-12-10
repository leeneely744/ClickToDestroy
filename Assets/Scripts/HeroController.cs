using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HeroController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;

    [Header("Combat")]
    [SerializeField] private int maxHp = 100;
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private float attackInterval = 0.8f;
    [SerializeField] private int maxConcurrentTargets = 1;

    private int currentHp;
    private float attackTimer;
    private readonly List<EnemyController> enemiesInRange = new List<EnemyController>();

    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 moveInput;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        currentHp = maxHp;
    }

    void Update()
    {
        ReadInput();
        HandleAttack();
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

    private void ReadInput()
    {
        if (Keyboard.current == null)
        {
            moveInput = Vector2.zero;
            return;
        }

        Vector2 input = Vector2.zero;

        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
        {
            input.y += 1f;
        }

        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
        {
            input.y -= 1f;
        }

        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
        {
            input.x += 1f;
        }

        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
        {
            input.x -= 1f;
        }

        moveInput = input.normalized;
    }

    private void HandleMovement()
    {
        if (rb == null)
        {
            return;
        }

        Vector2 newPosition = rb.position + moveInput * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(newPosition);

        if (animator != null)
        {
            bool isMoving = moveInput.sqrMagnitude > 0.0001f;
            animator.SetBool("isWalking", isMoving);
        }
    }

    private void HandleAttack()
    {
        if (enemiesInRange.Count == 0)
        {
            UpdateAttackAnimation(false);
            return;
        }

        attackTimer += Time.deltaTime;
        if (attackTimer < attackInterval)
        {
            return;
        }

        attackTimer = 0f;

        int targetCount = Mathf.Min(maxConcurrentTargets, enemiesInRange.Count);
        for (int i = 0; i < targetCount; i++)
        {
            EnemyController enemy = enemiesInRange[i];
            if (enemy == null || enemy.IsDead)
            {
                continue;
            }

            enemy.TakeDamage(attackDamage);
        }

        UpdateAttackAnimation(true);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.CompareTag("Enemy"))
        {
            return;
        }

        EnemyController enemy = col.GetComponent<EnemyController>();
        if (enemy != null && !enemiesInRange.Contains(enemy))
        {
            enemiesInRange.Add(enemy);
        }
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (!col.CompareTag("Enemy"))
        {
            return;
        }

        EnemyController enemy = col.GetComponent<EnemyController>();
        if (enemy != null)
        {
            enemiesInRange.Remove(enemy);
        }
    }

    public void TakeDamage(int damage)
    {
        if (currentHp <= 0)
        {
            return;
        }

        currentHp -= damage;
        if (currentHp <= 0)
        {
            currentHp = 0;
            if (animator != null)
            {
                animator.SetTrigger("Die");
            }

            enabled = false;
        }
    }

    private void UpdateAttackAnimation(bool isAttacking)
    {
        if (animator == null)
        {
            return;
        }

        animator.SetBool("isAttacking", isAttacking);
        if (isAttacking)
        {
            animator.SetBool("isWalking", false);
        }
    }
}

