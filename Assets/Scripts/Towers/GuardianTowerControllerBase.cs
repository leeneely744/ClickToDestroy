using System.Collections;
using UnityEngine;

public class GuardianTowerControllerBase : TowerController
{
    protected int maxUnits;
    private Coroutine guardianSpawnRoutine;

    protected override void Start()
    {
        base.Start();
        maxUnits = GetMaxUnits();
        ScheduleGuardianSpawn(1.0f); // 1秒後にガーディアンを生成開始
    }

    public override int GetMaxUnits()
    {
        return maxUnits;
    }

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

    public void OnGuardianDestroyed(int delaySeconds = 2)
    {
        // ガーディアンが破壊されたときに呼び出される
        ScheduleGuardianSpawn(delaySeconds); // 指定された秒数後に再生成をスケジュール
    }
}
