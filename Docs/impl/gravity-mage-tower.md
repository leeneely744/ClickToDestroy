# 重力魔法使いタワー 実装メモ

## 概要

`GravityMageTower.prefab` は `Docs/design/tower-fusions.csv` の ID `2F-6`（魔法＋砲台の合成）に対応するタワー。
他のタワーとの主な違いは以下の3点。

1. **プロジェクタイルを発射しない**（範囲内の敵に直接ダメージ）
2. **単体ではなく全敵にダメージ**（AttackRangeCircle内の非飛行敵すべて）
3. **攻撃のたびに道パネルが振動**し、スキル取得後は道の上にFireアニメーションを展開する

---

## クラス構成

```
GravityMageTower（Root GameObject）
  ├─ MagicTowerController        → 標準のタワー管理（ステータス読み込み・売却など）
  ├─ GravityMageAttackController → 攻撃ロジック（TowerAttackControllerを継承）
  ├─ GravityMageFireSkill        → スキル（TowerSkillを継承）
  ├─ GravityMageSpriteAnimator   → キャラクターのIdle/Attackスプライト切り替え
  └─ Rigidbody2D (GravityScale=0)
```

---

## 攻撃フロー

通常タワーは `Attack(EnemyController target)` で単一ターゲットに弾を撃つが、
GravityMage は `GravityMageAttackController` でこのメソッドを **override** して別の処理を行う。

```
TowerAttackController.TryAttack()   ← 攻撃間隔タイマー管理（変更なし）
  └─ Attack(enemiesInRange[0])      ← 引数は1体だが実際は使わない
       └─ GravityMageAttackController.Attack() [override]
            ├─ EnemiesInRange（範囲内全敵）をコピーしてダメージループ
            ├─ ShakeRoadsInRange()  ← DOTweenで道パネルを振動
            └─ PlayAttackAnimation() → GravityMageSpriteAnimator.PlayAttack()
```

`EnemiesInRange` は `TowerAttackController` の private フィールド `enemiesInRange` の
protected ゲッター（`IReadOnlyList<EnemyController>`）として公開している。
非飛行敵のみ含まれる（`canAttackFlying = false` がデフォルト）。

ダメージループは安全のためにイテレート前にリストをコピーする
（ダメージで敵がDestroyされると `OnTriggerExit2D` 経由でリストが変わる可能性があるため）。

---

## 道パネルの振動

`GameObject.Find("Roads")` でシーン上の Roads 親オブジェクトを取得し、
その直下の子Transformをループして攻撃範囲内のものを DOTween で振動させる。

```csharp
road.DOShakePosition(duration, strength, vibrato, randomness, fadeOut)
```

`worldRadius` は `Start()` 時に AttackRangeCircle の `CircleCollider2D.radius × lossyScale.x` から計算する。
プレハブの設定（スケール4、コライダー半径0.5）により worldRadius = 2.0。

---

## スキル「攻撃力UP」

`GravityMageFireSkill` は `TowerSkill` を継承し、購入時（`OnActivate()`）に次の2つを行う。

### 1. ダメージ倍増

`Owner`（= GravityMageAttackController）に `SetDamageMultiplier(2f)` を呼び、
以降の攻撃ダメージを10→20に倍増する。

### 2. Fireオーバーレイのスポーン

射程内の道パネル1つに対して `GravityMageFire.prefab` を1つインスタンス化する。
`GravityMageFire` は `GravityMageFireAnimator` を持ち、`Fire/1.png`〜`7.png` の7フレームをループ再生する。

スポーンした GameObject の参照は `GravityMageFireSkill` がリストで保持し、
タワーが破壊・売却されたとき（`OnDestroy()`）に一括で Destroy する。

```
Roads
  ├─ road_5_0          ← GravityMageFire をここにスポーン（射程内の場合）
  ├─ road_5_0 (1)      ← 同上
  └─ ...
```

---

## スプライトアニメーション

`GravityMageSpriteAnimator` はコルーチンベースで動作し、Unity の Animator コンポーネントは使用しない。

| 状態 | 使用フレーム | 枚数 |
|------|------------|------|
| Idle | 0_Magician_Girl_Idle_000〜017 | 18枚 |
| Attack | 0_Magician_Girl_Throwing_000〜011 | 12枚 |

- `OnEnable` でIdleループを開始、`OnDisable` で停止（SetActive対応）
- `PlayAttack()` を呼ぶとAttackアニメーションを1回再生し、Idleに戻る
- AttackアニメーションはIdleループのフレーム更新をマスクして優先される

---

## プレハブ設定値

| パラメータ | 値 |
|-----------|-----|
| 攻撃間隔 | 1.5秒 |
| 射程 | 2.0 Unity単位 |
| ダメージ/tick | 10（スキル後20） |
| 飛行敵対応 | 不可 |
| スキルコスト | 400 |
| 売却額 | 250 |

---

## 他タワーとの違い早見表

| 仕様 | 通常タワー（例: BowTower） | GravityMage |
|------|--------------------------|-------------|
| 攻撃方法 | Projectile をスポーン | 直接 TakeDamage() |
| 攻撃対象 | 範囲内の先頭1体 | 範囲内の全非飛行敵 |
| Animator | UnityのAnimator | コルーチン（GravityMageSpriteAnimator） |
| 攻撃演出 | 弾の飛翔 | 道パネルの振動（DOTween） |
| スキル演出 | なし or 弾の変化 | 道上のFireオーバーレイ展開 |
