# AGENTS

## 前提条件

このリポジトリの名前は ClickToDestroy です。
Issuesのページは https://github.com/leeneely744/ClickToDestroy/issues


## 必須指示
- 日本語で回答する。
- このプロジェクトはUnityの練習も兼ねているので、「どうしたら良いか？」という指示を受けたら、修正方針のヒントや方向性を示すだけにしてください。
- 「コード例を出して」という指示を受けたら、コードの修正例を表示してください。
- 「コードを修正してください」という指示が含まれていたら実際にコードを修正してください。
- `*.ai.md` という名前のファイルがあったら、その内容を読み込んだ上で回答してください。

## コーディングルール

### Nullチェック
- `[SerializeField]` フィールドや外部から注入される参照に対して null チェックを行う場合は、必ず `Debug.LogError` または `Debug.LogWarning` を出力すること。
- 黙って `return` するだけの null チェックは禁止。原因究明が困難になるため。
- エラーレベルの目安：
  - そのフィールドがないとスクリプトが正常動作しない → `Debug.LogError`
  - なくても動作するが意図しない状態になりうる → `Debug.LogWarning`
- 例：
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
