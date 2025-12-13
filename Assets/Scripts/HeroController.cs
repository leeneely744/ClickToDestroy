using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using Tags = Constants.Tags;

public class HeroController : MonoBehaviour, IPointerClickHandler, IDefender
{
    private static class AnimatorParams
    {
        public const string IsDead = "IsDead";
        public const string IsRunning = "IsRunning";
        public const string Attack = "Attack";
    }

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;

    [Header("Combat")]
    [SerializeField] private int maxHp = 100;
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private float attackInterval = 0.8f;
    [SerializeField] private int maxConcurrentTargets = 1;
    [SerializeField] private float refreshTime = 5f;
    private float refreshTimer = 0f; // 死んでから何秒経ったか

    private int currentHp;
    private float attackTimer;
    private readonly List<EnemyController> enemiesInRange = new List<EnemyController>();

    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 moveInput;

    private bool isMoving = false;
    private bool isMoveMode = false;
    private Vector3 moveTarget;
    private float moveDistance = 0.05f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        currentHp = maxHp;
    }

    void Update()
    {
        // 死亡中は復活タイマーだけ進める
        if (IsDead)
        {
            refreshTimer += Time.deltaTime;
            if (refreshTimer >= refreshTime)
            {
                refreshTimer = 0f;
                Revive();
            }
            return;
        }

        // 生存中はタイマーをリセット
        if (refreshTimer > 0f)
        {
            refreshTimer = 0f;
        }

        HandleAttack();
        HandleMovement();
        HandleHeroMoveInput();
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        isMoveMode = !isMoveMode;
    }

    // 移動モードで移動先を指定する
    private void HandleHeroMoveInput()
    {
        if (!isMoveMode || isMoving)
        {
            return;
        }

        if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame)
        {
            return;
        }

        // スマホようにUIタッチも含めて無視する場合はInput.GetTouch(0).fingerIdを使う。
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogWarning("Main Camera not found while handling hero move input.");
            return;
        }

        Vector2 screenPosition = Mouse.current.position.ReadValue();
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, mainCamera.nearClipPlane));
        worldPosition.z = 0f;

        moveTarget = worldPosition;
        isMoving = true;
        isMoveMode = false;
    }

    private void HandleMovement()
    {
        if (isMoving)
        {
            // 移動を開始したらすべての敵を攻撃対象から外す
            enemiesInRange.Clear();

            Vector3 direction = (moveTarget - transform.position).normalized;
            moveInput = new Vector2(direction.x, direction.y);

            if (animator != null)
            {
                animator.SetBool(AnimatorParams.IsRunning, true);
            }

            transform.position = Vector3.MoveTowards(transform.position, moveTarget, moveSpeed * Time.deltaTime);
            float distanceToTarget = Vector3.Distance(transform.position, moveTarget);
            if (distanceToTarget <= moveDistance)
            {
                // 移動完了
                isMoving = false;
                moveInput = Vector2.zero;
                animator?.SetBool(AnimatorParams.IsRunning, false);
            }
        }
    }

    private void HandleAttack()
    {
        if (isMoving)
        {
            return;
        }

        if (enemiesInRange.Count == 0)
        {
            return;
        }

        attackTimer += Time.deltaTime;
        if (attackTimer < attackInterval)
        {
            return;
        }

        // ここから攻撃開始
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
        if (isMoving)
        {
            return;
        }

        if (!col.CompareTag(Tags.Enemy))
        {
            return;
        }

        EnemyController enemy = col.GetComponent<EnemyController>();
        if (enemy != null && !enemiesInRange.Contains(enemy))
        {
            enemiesInRange.Add(enemy);
            enemy.EngageDefender(this);
        }
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (!col.CompareTag(Tags.Enemy))
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
                animator.SetBool(AnimatorParams.IsDead, true);
            }

            // 死亡時は「死亡中」モーションを再生したいので、
            // スクリプト自体は有効なままにしておく。
            isMoving = false;
            isMoveMode = false;
            enemiesInRange.Clear();
        }
    }

    private void UpdateAttackAnimation(bool isAttacking)
    {
        if (animator == null)
        {
            return;
        }

        // 攻撃は単発モーションなので Trigger を使う
        if (!isAttacking || animator == null)
        {
            return;
        }

        animator.SetTrigger(AnimatorParams.Attack);
    }

    /// <summary>
    /// Hero を復活させる処理をまとめたメソッド。
    /// HP やアニメーション状態、内部フラグを初期状態に戻します。
    /// </summary>
    public void Revive()
    {
        // HP を最大値に戻す
        currentHp = maxHp;

        // 攻撃用タイマー・移動状態をリセット
        attackTimer = 0f;
        isMoving = false;
        isMoveMode = false;
        enemiesInRange.Clear();

        // アニメーションパラメータのリセット
        if (animator != null)
        {
            animator.SetBool(AnimatorParams.IsDead, false);
            animator.SetBool(AnimatorParams.IsRunning, false);
        }
    }

    public bool IsDead => currentHp <= 0;
}
