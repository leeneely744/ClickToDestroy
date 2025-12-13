using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using Tags = Constants.Tags;

// TODO(Hero 移動仕様メモ)
// - Hero をクリックしたら「Hero移動モード」に入る（次のクリックで目的地を決める状態）
// - 移動モード中にフィールド上を左クリックした位置を ScreenToWorldPoint でワールド座標に変換し、moveTarget(Vector3)として記録する
//   - target = Camera.main.ScreenToWorldPoint(Input.mousePosition); target.z = transform.position.z; のように z は現在値に合わせる
// - moveTarget が決まったら isMoving = true にして、isMoveMode = false（次のクリック待ち状態は終了）
// - FixedUpdate で transform.position を Vector3.MoveTowards(transform.position, moveTarget, moveSpeed * Time.fixedDeltaTime) で移動させる
// - Vector3.Distance(transform.position, moveTarget) <= stopDistance になったら移動完了とみなし、isMoving = false にする
// - Animator がある場合、isMoving 中だけ isWalking フラグを true にし、停止時は false に戻す

public class HeroController : MonoBehaviour, IPointerClickHandler, IDefender
{
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
                animator.SetBool("isRunning", true);
            }

            transform.position = Vector3.MoveTowards(transform.position, moveTarget, moveSpeed * Time.deltaTime);
            float distanceToTarget = Vector3.Distance(transform.position, moveTarget);
            if (distanceToTarget <= moveDistance)
            {
                // 移動完了
                isMoving = false;
                moveInput = Vector2.zero;
                animator?.SetBool("isRunning", false);
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
            UpdateAttackAnimation(false);
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
                animator.SetTrigger("Die");
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

        animator.SetBool("isAttacking", isAttacking);
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
            animator.ResetTrigger("Die");
            animator.SetBool("isRunning", false);
            animator.SetBool("isAttacking", false);
        }
    }

    public bool IsDead => currentHp <= 0;
}
