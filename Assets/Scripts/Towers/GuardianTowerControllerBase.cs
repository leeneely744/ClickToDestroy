using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    protected override void Start()
    {
        base.Start();
        ScheduleGuardianSpawn(0.0f); // 最初は即時生成
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
        List<Vector3> positions = new List<Vector3>();
        Transform anchorPoint = null;
        if (CurrentTowerPlace != null)
        {
            anchorPoint = CurrentTowerPlace.transform.Find("InitialGuardianPoint");
        }

        Vector3 basePosition = anchorPoint != null ? anchorPoint.position : transform.position;
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
            Vector3 destination = targetPositions[i];
            guardian.SetMoveTarget(destination);
        }
    }

    public void OnGuardianDestroyed(float delaySeconds = 2)
    {
        // ガーディアンが破壊されたときに呼び出される
        ScheduleGuardianSpawn(delaySeconds); // 指定された秒数後に再生成をスケジュール
    }

    public void StartMoveMode()
    {
        Debug.Log("Starting guardian move mode");
        // 衛兵移動モードを開始する

        // プレイヤーがタワーのAttackRangeCircle内部をクリックすると
        // その場所に衛兵を移動させるようにする。
        // もしクリックした場所がAttackRangeCircleの外側なら
        // クリックした場所にバツマークを表示する。

        // 移動が終わったら元のモードに戻す。
    }
}
