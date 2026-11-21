# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 言語

日本語で回答すること。

## 修正方針の提案

修正方針・リファクタリング方針・設計案・実装方針をユーザーに提示するときは、**必ず** `adversarial-plan-review` スキル（`.claude/skills/adversarial-plan-review/SKILL.md`）を使い、提示した方針に対する敵対的レビューを行ってから出すこと。規模が小さい修正でも省略しない。plan mode や brainstorming / writing-plans の中で方針を出す場合も同様。

## プロジェクト概要

Unity 2D タワーディフェンスゲーム。タワーを配置して敵の侵攻を食い止める。Kingdom Rush を参考にしたデザイン。

- **Unity バージョン**: URP 17.2.0 (Unity 6 系)
- **主要パッケージ**: Input System 1.14.2, DOTween (Demigiant), UIEffect (mob-sakai)
- **スクリプト言語**: C# (Assembly-CSharp)

## 開発コマンド

Unity Editor から Play ボタンで実行。CLI ビルドは未設定。

pre-commit フックが有効：

```bash
pre-commit run --all-files   # 全ファイルに対してフック実行
pre-commit run               # ステージ済みファイルのみ
```

フックの内容: trailing-whitespace, end-of-file-fixer, YAML 検証, 大容量ファイルチェック, gitleaks（シークレット漏洩検出）。

## アーキテクチャ

### コアシステム

**GameManager** (`Assets/Scripts/GameManager.cs`) — シングルトン。HP / 所持金 / ウェーブ UI の参照を持ち、ゲームオーバー・クリア・リトライを制御する。`Time.timeScale` でゲームの一時停止を行う。

**EnemySpawner** (`Assets/Scripts/EnemySpawner.cs`) — `LevelAsset`（ScriptableObject）を受け取りコルーチンでウェーブを順番に実行する。`activeEnemies` カウンタが 0 になるまで次のウェーブ開始を待つ。全ウェーブ完了後に `GameManager.HandleGameClear()` を呼ぶ。

### ウェーブ・ルートシステム

`LevelAsset` → `WaveAsset[]` → `SpawnGroup[]` という ScriptableObject の階層で敵のスポーン定義を管理する（`Assets/Scripts/Spawning/`）。

- **`RouteAsset`** — シーン上の `Route` と紐づける識別子 ScriptableObject。WaveAsset（アセット）からシーン上の `Route`（MonoBehaviour）を直接参照できないためこの間接参照を使う。
- **`RouteRegistry`** — シーン上の `Route` を起動時に登録し、`RouteAsset` を Key にして解決する。
- **ルートの多様性**: 1 本の道に `Route_A_1`, `Route_A_2` など複数の Route を敷き、SpawnGroup ごとに使用レーンを指定する（Kingdom Rush 方式）。敵個体に横オフセットは付けない。

### タワーシステム

**`TowerController`** — すべてのタワーの基底クラス。`TowerStats`（ScriptableObject）でレベルごとのコスト・射程・攻撃間隔・次レベル Prefab を管理する。アップグレード時は現 GameObject を破棄して次レベル Prefab をインスタンス化する。

**`TowerAttackController`** — 攻撃責務を分離したコンポーネント。`AttackRangeCircle` 子オブジェクトの `CircleCollider2D` トリガーで敵を検知し、`Projectile` を射出する。`canAttackFlying` フラグで飛行敵の攻撃可否を切り替える。

**`TowerSkill`** — `TowerAttackController` と同じ GameObject にアタッチする abstract クラス。`OnInitialize` / `OnAttack` / `OnActivate` フックを持つ。購入制のスキルは `IPurchasableSkill` を実装する。

**`GuardianTowerControllerBase`** → **`GuardianController`** — 兵士排出タワーの基底。`SpawnGuardians()` をコルーチンで遅延実行し、兵士の死亡時は `OnGuardianDestroyed` からリスポーンをスケジュールする。`savedGuardianCenter` でアップグレード後も兵士の集合位置を引き継ぐ。兵士の移動先指定（Move モード）は `Update` ループ内でマウス入力を検知して処理する。

**`UnitRangedAttack`** — 兵士ユニットが独自に遠距離攻撃するためのコンポーネント（例: 忍者のクナイ）。`GuardianController` が近接ターゲットを持っている間は `enabled = false` にして近接攻撃を優先させる。

### タワー合成システム

2 種・3 種・4 種のタワーをドラッグ&ドロップで合成できる。設計詳細は `Docs/design/tower-fusion-rules.md` と `Docs/design/tower-fusions.csv` を参照。

- **`TowerFusionRecipe`** — 合成元タワーの `towerId` リストと合成結果 Prefab を持つ ScriptableObject。
- **`TowerFusionService`** — `Resources/TowerFusion/` から全レシピを読み込み、`CanFuse(a, b, out recipe)` で合成判定する純粋クラス。
- **`FusionManager`** — シングルトン。`TryFuse` で合成元・合成先を破棄し、結果タワーをインスタンス化して `TowerPlace` を引き渡す。
- **`DragController`** — ドラッグ中の半透明追従と、ドロップ先タワーの合成可否表示（緑/赤ハイライト）を担当する。

### 敵システム

**`EnemyController`** — `EnemyData`（ScriptableObject）から HP・速度・飛行フラグ・報酬を初期化する。`FixedUpdate` でウェイポイントを順に追跡する。ゴールに到達すると `ScoreBoard.CalcHp` を呼んで HP を減らす。

**`EnemyAttackController`** — 遠距離攻撃と近接攻撃を担当するコンポーネント。近接攻撃を優先し、近接範囲外の場合のみ遠距離攻撃を行う。交戦中（`IsEngaged`）は `EnemyController` の移動が止まる。

**攻撃属性**: `AttackType.Physical` / `AttackType.Magical` の 2 種。`EnemyData` の `physicalResistance` / `magicalResistance`（0〜1 の係数）でダメージ軽減。

### ヒーローシステム

**`HeroController`** — プレイヤーが移動先を指定できる自律戦闘ユニット。移動モード中は敵の `DisengageDefender()` を呼んで交戦を解除する。死亡後 `refreshTime` 秒で自動復活する。`HeroButton` を通じてスキルエフェクト表示と連動する。

### UI・クリック処理

**`TowerPlace`** — 空きスロットのクリックで `TowerSelectPanel.Show()` を呼ぶ。占有中はクリックを無視する。

**`StatusClickHandler`** (`IPointerClickHandler` 実装) — `EnemyController`, `HeroController`, `GuardianController` の子 Collider に動的アタッチされ、クリック時に `StatusPanel` へ `IStatusProvider.GetStatusInfo()` を渡す。

**`TowerActionPanel`** — 選択中タワーの売却・アップグレード・スキル購入・兵士移動ボタンを表示するシングルトン UI。

### インターフェース

- **`IDefender`** — `TakeDamage(int)` と `IsDead` を持つ。Hero / Guardian が実装。敵の攻撃対象の抽象化。
- **`IStatusProvider`** — `GetStatusInfo()` を持つ。タワー・敵・Hero / Guardian が実装。クリック時のステータス表示に使用。
- **`IPurchasableSkill`** — `TryPurchase(Money)` を持つ。`TowerSkill` と `GuardianSkill` が実装。

## コーディングルール

### Null チェック

`[SerializeField]` フィールドや外部注入の参照に対して null チェックを行う場合は、必ず `Debug.LogError` または `Debug.LogWarning` を出力すること。黙って `return` するだけの null チェックは禁止。

```csharp
// NG
if (heroButton == null) return;

// OK
if (heroButton == null)
{
    Debug.LogError("[HeroController] HeroButton が設定されていません。Inspector を確認してください。", this);
    return;
}
```

エラーレベルの判断基準:
- スクリプトが正常動作しない → `Debug.LogError`
- なくても動作するが意図しない状態になりうる → `Debug.LogWarning`

### `*.ai.md` ファイル

`*.ai.md` という名前のファイルがあったら、その内容を読み込んだ上で回答・実装すること。設計ドキュメントとして扱う（例: `Docs/design_of_tower_fusion.ai.md`）。

## 設計ドキュメント

- `Docs/decisions.md` — 設計判断ログ（Route の多様性確保・敵の遠距離攻撃・合成ドラッグ仕様など）
- `Docs/design/tower-fusion-rules.md` — 合成ティア・スキル数・タワー役割の設計ルール
- `Docs/design/tower-fusions.csv` — 合成タワー一覧（ID・親タワー・スキル詳細）
- `Docs/design/gravity-mage-tower.md` — 重力魔法使いタワーの詳細設計
