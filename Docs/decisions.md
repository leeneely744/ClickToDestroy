# 設計判断ログ

このプロジェクトで採用した設計判断と、その理由を記録するドキュメント。
新しい判断が出てきたら、このファイルの末尾にセクションを追加する。

---

## Route の多様性確保: 1本の道に複数Routeを設置する

### 決定
1本の道に対して複数の Route（例: `Route_A_1`, `Route_A_2`, ...）を配置し、
`EnemySpawner` は敵スポーン時にそれらをランダムまたは指定で選択する。

各 Enemy が同じ Route を共有することで発生する「位置の重なり」は、
Route 側を増やすことで解消する方針とする。

### 理由
- 参考タイトルである **Kingdom Rush** がこの方式を採用している。
  道幅の中に2〜3レーンを敷く形式。
- Enemy 側で横オフセットや揺らぎを実装する案も検討したが、
  Kingdom Rush の挙動と揃えるため採用しなかった。
- レーンを明示的に作ることで、デザイナがレベルごとに
  「どのレーンを使うか」を Inspector で完全に制御でき、
  挙動が予測しやすい。

### 棄却した案
- Enemy ごとに ±N の横オフセットを乱数で固定する案
- Perlin ノイズで横揺れさせる案
- 個体ごとに速度・スケールをばらつかせる案

これらは Kingdom Rush 風のレーン感が出ないため採用せず。

---

## 敵の遠距離攻撃: EnemyAttackController + EnemyProjectile

### 決定

遠距離攻撃ロジックを `EnemyAttackController` に追加し、投射物は専用クラス `EnemyProjectile` として実装した。

### 設定方法（Inspector）

**EnemyData（ScriptableObject）**

| フィールド | 説明 |
|---|---|
| `rangedAttack.damage` | 遠距離攻撃のダメージ。0 にすると遠距離攻撃無効 |
| `rangedAttack.attackInterval` | 発射間隔（秒） |
| `rangedAttack.attackRange` | 射程距離（Unity units）。この半径内の Defender を狙う |
| `meleeAttack.attackRange` | 近接攻撃の自律検知半径。0 の場合は Guardian の物理接触に依存（既存挙動） |

**Enemy Prefab の EnemyAttackController**

| フィールド | 説明 |
|---|---|
| `Ranged Projectile Prefab` | `EnemyProjectile` コンポーネントをアタッチした弾 Prefab |
| `Ranged Fire Point` | 発射位置の Transform。null の場合は敵の原点から発射（ピボットが中心付近なら許容範囲） |
| `Defender Layer Mask` | `Clickable` レイヤーを指定（Hero・Guardian がこのレイヤーを使用） |

**EnemyProjectile Prefab**

| フィールド | 説明 |
|---|---|
| `Speed` | 飛翔速度（Unity units/秒） |
| `Max Lifetime` | 自動消滅までの時間（秒） |

### 優先度ルール

- `meleeAttack.attackRange` 以内に Defender がいれば**近接攻撃を優先**し、遠距離攻撃はスキップ
- 近接範囲外かつ `rangedAttack.attackRange` 以内なら遠距離攻撃を実行
- 近接・遠距離どちらかが交戦中は `IsEngaged = true` になり、敵の移動が停止する

### Defender の検出

`Physics2D.OverlapCircleAll(defenderLayerMask)` でヒットしたオブジェクトを `GetComponent<IDefender>()` でフィルタリングする。Clickable レイヤーにタワーなど非 Defender が混在していても、`IDefender` 未実装のオブジェクトは自動的に無視される。

### 棄却した案

- 既存の `Projectile.cs`（タワー用）を流用する案 → ターゲットが `EnemyController` 固定のため不採用。`EnemyProjectile` を分離した。
- 遠距離攻撃中も移動を続ける案 → `IsEngaged` を統一することで既存の移動停止ロジックをそのまま活用できるため、停止する方針を採用。
