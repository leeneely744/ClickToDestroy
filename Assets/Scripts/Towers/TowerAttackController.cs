using UnityEngine;

/// <summary>
/// タワーの「攻撃する」責務だけを担当するコンポーネント。
/// まずは空の土台として定義し、少しずつ TowerController から攻撃ロジックを移していきます。
/// </summary>
public class TowerAttackController : MonoBehaviour
{
    public void Configure(float attackInterval, float range)
    {
        // 攻撃間隔や射程の設定を行うロジックをここに実装します。
        // 例えば、攻撃タイマーの初期化や範囲コライダーの設定など。
    }

    public void SetProjectile(GameObject projectilePrefab, Transform firePoint, float projectileTravelTime)
    {
        // 砲弾の設定を行うロジックをここに実装します。
        // 例えば、砲弾の発射位置や速度の設定など。
    }

    private void Update()
    {
        // 内部でタイマーをすすめ、一定時間ごとにTryAttack()を呼ぶ
    }

    private void TryAttack()
    {
        // 射程内の敵を検出し、最も優先度の高い敵を攻撃するロジックをここに実装します。
    }

    private void Attack(EnemyController target)
    {
        // 実際に敵を攻撃するロジックをここに実装します。
        // 例えば、砲弾を生成してターゲットに向かって発射するなど。
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 射程内に敵が入ったときの処理をここに実装します。
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // 射程外に敵が出たときの処理をここに実装します。
    }
}

