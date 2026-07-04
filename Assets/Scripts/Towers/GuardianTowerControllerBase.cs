using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GuardianTowerControllerBase : TowerController
{
    [SerializeField] protected int maxSoldiers = 3;
    private Coroutine guardianSpawnRoutine;
    protected virtual GameObject GuardianPrefab => null;
    protected string[] guardianNames =
    {
        "Sato",
        "Suzuki",
        "Takahashi",
        "Tanaka",
        "Watanabe"
    };

    private bool isMoveModeActive;
    protected Vector3? savedGuardianCenter;

    protected override void Start()
    {
        base.Start();
        ScheduleGuardianSpawn(0.0f); // 最初は即時生成
    }

    protected override void Update()
    {
        base.Update();

        if (isMoveModeActive)
        {
            HandleGuardianMoveInput();
        }
    }

    public override int GetMaxUnits()
    {
        return MaxSoldiers;
    }

    protected virtual int MaxSoldiers => maxSoldiers;

    protected void ScheduleGuardianSpawn(float delaySeconds = 0f)
    {
        if (guardianSpawnRoutine != null)
        {
            StopCoroutine(guardianSpawnRoutine);
        }

        guardianSpawnRoutine = StartCoroutine(SpawnGuardiansAfterDelay(delaySeconds));
    }

    private IEnumerator SpawnGuardiansAfterDelay(float delaySeconds)
    {
        if (delaySeconds > 0f)
        {
            yield return new WaitForSeconds(delaySeconds);
        }

        SpawnGuardians();
    }

    protected virtual void SpawnGuardians()
    {
        // GuardianTower 派生クラスで具体的な生成ロジックを実装する
    }

    protected List<Vector3> BuildInitialGuardianPositions()
    {
        Vector3 basePosition;
        if (savedGuardianCenter.HasValue)
        {
            basePosition = savedGuardianCenter.Value;
        }
        else
        {
            Transform anchorPoint = CurrentTowerPlace != null
                ? CurrentTowerPlace.transform.Find("InitialGuardianPoint")
                : null;
            basePosition = anchorPoint != null ? anchorPoint.position : transform.position;
        }

        List<Vector3> positions = new List<Vector3>();
        for (int i = 0; i < MaxSoldiers; i++)
        {
            Vector2 offset = Random.insideUnitCircle * 0.5f;
            positions.Add(basePosition + new Vector3(offset.x, offset.y, 0f));
        }

        return positions;
    }

    protected void MoveGuardians(GuardianController[] guardians, IList<Vector3> targetPositions)
    {
        if (targetPositions == null || targetPositions.Count == 0)
        {
            Debug.LogWarning($"MoveGuardians called without target positions on {name}.");
            return;
        }

        for (int i = 0; i < guardians.Length; i++)
        {
            GuardianController guardian = guardians[i];
            Vector3 destination = targetPositions[Mathf.Min(i, targetPositions.Count - 1)];
            guardian.SetMoveTarget(destination);
        }
    }

    protected override void OnUpgradeTo(TowerController newController)
    {
        if (!savedGuardianCenter.HasValue) return;
        if (newController is GuardianTowerControllerBase newGuardian)
        {
            newGuardian.savedGuardianCenter = savedGuardianCenter;
        }
    }

    public void OnGuardianDestroyed(float delaySeconds = 2)
    {
        ScheduleGuardianSpawn(delaySeconds);
    }

    // スポーン済みの兵士インスタンスからスキルを取得する。
    // プレハブから取得すると TryPurchase で isPurchased がプレハブ本体に書き込まれ
    // ゲーム再起動まで残り続けるため、必ずインスタンスから取得する。
    public override IPurchasableSkill[] GetSkills()
    {
        var guardian = GetComponentInChildren<GuardianController>();
        if (guardian != null)
            return guardian.GetComponents<GuardianSkill>().Cast<IPurchasableSkill>().ToArray();

        // 兵士未スポーン時のフォールバック（コスト・名前の表示用）
        if (GuardianPrefab == null) return System.Array.Empty<IPurchasableSkill>();
        return GuardianPrefab.GetComponents<GuardianSkill>().Cast<IPurchasableSkill>().ToArray();
    }

    // 購入後、現在シーンに存在する兵士にもスキルを伝播する
    public override bool TryPurchaseSkill(int index)
    {
        if (!base.TryPurchaseSkill(index)) return false;

        foreach (var guardian in GetComponentsInChildren<GuardianController>())
        {
            var guardianSkills = guardian.GetComponents<GuardianSkill>();
            if (index < guardianSkills.Length)
                guardianSkills[index].Activate();
        }
        return true;
    }

    public virtual void StartMoveMode()
    {
        MoveModeCoordinator.Activate(this, CancelMoveMode);
        isMoveModeActive = true;
    }

    // 他のユニットが移動モードを開始したときに MoveModeCoordinator から呼ばれる
    private void CancelMoveMode()
    {
        isMoveModeActive = false;
    }

    protected virtual void ExitMoveMode()
    {
        isMoveModeActive = false;
        MoveModeCoordinator.Deactivate(this);
    }

    private void HandleGuardianMoveInput()
    {
        if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame)
        {
            return;
        }

        // Physics2DRaycaster も IsPointerOverGameObject に含まれるため、
        // UI（GraphicRaycaster）のみをチェックして移動入力をブロックする。
        if (IsPointerOverUI())
        {
            return;
        }

        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogWarning("Main Camera not found while handling guardian move input.");
            ExitMoveMode();
            return;
        }

        Vector2 screenPosition = Mouse.current.position.ReadValue();
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, mainCamera.nearClipPlane));
        worldPosition.z = 0f;

        if (IsWithinMoveRange(worldPosition))
        {
            MoveGuardiansTo(worldPosition);
            ExitMoveMode();
        }
        else
        {
            ShowInvalidMoveFeedback(worldPosition);
            ExitMoveMode();
        }
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

    protected virtual bool IsWithinMoveRange(Vector3 targetPosition)
    {
        return Vector2.Distance(transform.position, targetPosition) <= AttackRangeWorldRadius;
    }

    protected virtual void MoveGuardiansTo(Vector3 targetPosition)
    {
        GuardianController[] guardians = GetComponentsInChildren<GuardianController>();
        if (guardians.Length == 0)
        {
            return;
        }

        savedGuardianCenter = targetPosition;

        var positions = new List<Vector3>();
        for (int i = 0; i < guardians.Length; i++)
        {
            Vector2 offset = Random.insideUnitCircle * 0.4f;
            positions.Add(targetPosition + new Vector3(offset.x, offset.y, 0f));
        }

        MoveGuardians(guardians, positions);
    }

    protected virtual void ShowInvalidMoveFeedback(Vector3 invalidPosition)
    {
        Debug.Log($"Invalid guardian move position {invalidPosition} for {name}");
        // TODO: プレイヤーに無効な移動範囲であることをフィードバックするエフェクトなどを実装
    }
}
