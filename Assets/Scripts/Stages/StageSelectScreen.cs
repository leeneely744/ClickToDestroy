using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// ステージセレクト画面。StageCatalog を参照して各ステージのボタンを生成し、
/// 押下で GameFlow.LoadStage を呼ぶ最小実装。
/// </summary>
public class StageSelectScreen : MonoBehaviour
{
    [SerializeField] private StageCatalog catalog;
    [SerializeField] private Transform buttonContainer;
    [SerializeField] private Button stageButtonPrefab;

    private void Start()
    {
        if (catalog == null)
        {
            Debug.LogError("[StageSelectScreen] catalog が設定されていません。Inspector で StageCatalog を割り当ててください。", this);
            return;
        }

        if (buttonContainer == null)
        {
            Debug.LogError("[StageSelectScreen] buttonContainer が設定されていません。Inspector を確認してください。", this);
            return;
        }

        if (stageButtonPrefab == null)
        {
            Debug.LogError("[StageSelectScreen] stageButtonPrefab が設定されていません。Inspector を確認してください。", this);
            return;
        }

        BuildButtons();
    }

    private void BuildButtons()
    {
        if (catalog.stages == null)
        {
            Debug.LogWarning("[StageSelectScreen] catalog.stages が未設定です。ステージボタンは生成されません。", this);
            return;
        }

        foreach (var entry in catalog.stages)
        {
            if (entry == null)
            {
                Debug.LogWarning("[StageSelectScreen] catalog.stages に null 要素があります。スキップします。", this);
                continue;
            }

            // onClick のクロージャ捕捉用にローカルへコピーする（foreach 変数を直接捕捉しない）
            StageAsset stage = entry;

            Button button = Instantiate(stageButtonPrefab, buttonContainer);

            var label = button.GetComponentInChildren<TMP_Text>();
            if (label != null)
            {
                label.text = string.IsNullOrEmpty(stage.displayName) ? stage.stageId : stage.displayName;
            }

            button.onClick.AddListener(() => GameFlow.LoadStage(stage));
        }
    }
}
