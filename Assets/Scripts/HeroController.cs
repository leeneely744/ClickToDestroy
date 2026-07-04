using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Tags = Constants.Tags;

[RequireComponent(typeof(Animator))]
public class HeroController : MonoBehaviour, IDefender, IStatusProvider
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

    public bool IsDead => currentHp <= 0;

    private int currentHp;
    private float timeSinceLastAttack;
    private readonly List<EnemyController> enemiesInRange = new List<EnemyController>();

    private Animator animator;

    private bool isMoving = false;
    private bool isMoveMode = false;
    private Vector3 moveTarget;
    private float moveDistance = 0.05f;
    [SerializeField] private HealthBarController healthBar;
    [SerializeField] private bool canAttackFlying = true;

    public event System.Action<bool> OnDeadStateChanged;
    [SerializeField] private HeroButton heroButton;

    void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (animator == null)
        {
            Debug.LogError("HeroController には Animator が必須です。", this);
            enabled = false;
            return;
        }

        currentHp = maxHp;
        if (healthBar == null)
        {
            healthBar = GetComponentInChildren<HealthBarController>();
        }

        UpdateHealthBar();
        RegisterClickHandlers();
    }

    private void RegisterClickHandlers()
    {
        foreach (var col in GetComponentsInChildren<Collider2D>())
        {
            if (col.GetComponent<StatusClickHandler>() == null)
                col.gameObject.AddComponent<StatusClickHandler>();
        }
    }

    public StatusInfo GetStatusInfo()
    {
        var sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();
        return new StatusInfo
        {
            displayName = gameObject.name.Replace("(Clone)", "").Trim(),
            icon = sr != null ? sr.sprite : null,
            maxHp = maxHp,
            getCurrentHp = () => currentHp,
            attackDamage = attackDamage > 0 ? attackDamage : (int?)null,
            physicalResistance = 0f,
            magicalResistance = 0f,
        };
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

        TryAttack();
        HandleMovement();
        HandleHeroMoveInput();
    }

    public void Select()
    {
        if (IsDead) return;
        MoveModeCoordinator.Activate(this, CancelMoveMode);
        isMoveMode = true;
    }

    // 他のユニットが移動モードを開始したときに MoveModeCoordinator から呼ばれる
    private void CancelMoveMode()
    {
        isMoveMode = false;
        if (heroButton == null)
        {
            Debug.LogWarning("[HeroController] HeroButton が設定されていません。Inspector を確認してください。", this);
            return;
        }
        heroButton.DeactivateEffect();
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

        if (IsPointerOverUI())
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
        MoveModeCoordinator.Deactivate(this);
        if (heroButton == null)
        {
            Debug.LogError("[HeroController] HeroButton が設定されていません。Inspector を確認してください。", this);
            return;
        }
        heroButton.DeactivateEffect();
    }

    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;
        var pointer = new PointerEventData(EventSystem.current)
        {
            position = Mouse.current.position.ReadValue()
        };
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointer, results);
        return results.Any(r => r.module is GraphicRaycaster);
    }

    private void HandleMovement()
    {
        if (isMoving)
        {
            // 移動を開始したらすべての敵を攻撃対象から外す
            foreach (var enemy in enemiesInRange)
            {
                if (enemy != null) enemy.DisengageDefender();
            }
            enemiesInRange.Clear();

            SwitchRunning(true);

            transform.position = Vector3.MoveTowards(transform.position, moveTarget, moveSpeed * Time.deltaTime);
            float distanceToTarget = Vector3.Distance(transform.position, moveTarget);
            if (distanceToTarget <= moveDistance)
            {
                // 移動完了
                isMoving = false;
                SwitchRunning(false);
            }
        }
    }

    private void TryAttack()
    {
        if (isMoving)
        {
            return;
        }

        if (enemiesInRange.Count == 0)
        {
            return;
        }

        timeSinceLastAttack += Time.deltaTime;
        if (timeSinceLastAttack < attackInterval)
        {
            return;
        }

        // ここから攻撃開始
        timeSinceLastAttack = 0f;

        int targetCount = Mathf.Min(maxConcurrentTargets, enemiesInRange.Count);
        for (int i = 0; i < targetCount; i++)
        {
            EnemyController enemy = enemiesInRange[i];
            if (enemy == null || enemy.IsDead)
            {
                continue;
            }

            if (enemy.IsFlying && !canAttackFlying)
            {
                continue;
            }

            enemy.EngageDefender(this);
            enemy.TakeDamage(attackDamage);
        }

        TriggerAttackAnimation();
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
        if (enemy == null)
        {
            return;
        }

        // 飛行ユニットを攻撃しない設定なら、そもそもターゲット登録しない
        if (enemy.IsFlying && !canAttackFlying)
        {
            return;
        }

        if (!enemiesInRange.Contains(enemy))
        {
            enemiesInRange.Add(enemy);
            if (enemiesInRange.Count <= maxConcurrentTargets)
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
            SwitchDead(true);

            // 死亡時は「死亡中」モーションを再生したいので、
            // スクリプト自体は有効なままにしておく。
            isMoving = false;
            isMoveMode = false;
            MoveModeCoordinator.Deactivate(this);
            enemiesInRange.Clear();
            OnDeadStateChanged?.Invoke(true);
        }

        UpdateHealthBar();
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
        timeSinceLastAttack = 0f;
        isMoving = false;
        isMoveMode = false;
        enemiesInRange.Clear();

        // アニメーションパラメータのリセット
        SwitchDead(false);
        SwitchRunning(false);

        OnDeadStateChanged?.Invoke(false);
        UpdateHealthBar();
    }

    private void SwitchDead(bool isDead)
    {
        animator.SetBool(AnimatorParams.IsDead, isDead);
    }

    private void SwitchRunning(bool isRunning)
    {
        animator.SetBool(AnimatorParams.IsRunning, isRunning);
    }

    private void TriggerAttackAnimation()
    {
        animator.SetTrigger(AnimatorParams.Attack);
    }

    private static class AnimatorParams
    {
        public const string IsDead = "IsDead";
        public const string IsRunning = "IsRunning";
        public const string Attack = "Attack";
    }

    private void UpdateHealthBar()
    {
        if (healthBar == null)
        {
            return;
        }

        float ratio = maxHp > 0 ? (float)currentHp / maxHp : 0f;
        healthBar.SetRatio(ratio);
    }
}
