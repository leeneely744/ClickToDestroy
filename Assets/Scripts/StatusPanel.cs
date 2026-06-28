using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

// ======================================================
// Unity エディタでの UI セットアップ手順:
//
// 1. Canvas 内に空の GameObject "StatusPanel" を作成し、このスクリプトをアタッチ
// 2. 以下の子オブジェクト構成を作る（全て Canvas の子に配置）:
//
//   StatusPanel (このスクリプト, Image, anchored bottom)
//     ├── IconImage      : Image コンポーネント (80x80)
//     ├── NameText       : TextMeshProUGUI
//     ├── HpGroup        : GameObject
//     │    └── HpText    : TextMeshProUGUI
//     ├── AttackGroup    : GameObject
//     │    └── AttackText: TextMeshProUGUI
//     ├── PhysDefGroup   : GameObject
//     │    └── PhysDefText: TextMeshProUGUI
//     └── MagicDefGroup  : GameObject
//          └── MagicDefText: TextMeshProUGUI
//
// 3. Inspector で各フィールドを対応する子オブジェクトに紐付ける
// 4. PanelRect には StatusPanel 自身の RectTransform を設定する
// ======================================================
public class StatusPanel : MonoBehaviour
{
    public static StatusPanel Instance { get; private set; }

    [Header("Panel")]
    [SerializeField] private RectTransform panelRect;

    [Header("Header")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;

    [Header("HP")]
    [SerializeField] private GameObject hpGroup;
    [SerializeField] private TMP_Text hpText;

    [Header("Attack")]
    [SerializeField] private GameObject attackGroup;
    [SerializeField] private TMP_Text attackText;

    [Header("Physical Defense")]
    [SerializeField] private GameObject physDefGroup;
    [SerializeField] private TMP_Text physDefText;

    [Header("Magic Defense")]
    [SerializeField] private GameObject magicDefGroup;
    [SerializeField] private TMP_Text magicDefText;

    private StatusInfo currentInfo;
    private Object providerObject;
    private bool justShown;
    private Canvas canvas;

    private void Awake()
    {
        Instance = this;
        canvas = GetComponentInParent<Canvas>();
        gameObject.SetActive(false);
    }

    public void Show(IStatusProvider provider)
    {
        currentInfo = provider.GetStatusInfo();
        providerObject = provider as Object;
        justShown = true;

        if (iconImage != null)
        {
            iconImage.sprite = currentInfo.icon;
            iconImage.enabled = currentInfo.icon != null;
        }

        if (nameText != null)
            nameText.text = currentInfo.displayName;

        bool showHp = currentInfo.maxHp.HasValue && currentInfo.getCurrentHp != null;
        if (hpGroup != null) hpGroup.SetActive(showHp);
        if (showHp && hpText != null)
            hpText.text = $"{currentInfo.getCurrentHp()} / {currentInfo.maxHp}";

        bool showAtk = currentInfo.attackDamage.HasValue;
        if (attackGroup != null) attackGroup.SetActive(showAtk);
        if (showAtk && attackText != null)
            attackText.text = currentInfo.attackDamage.ToString();

        bool showPhysDef = currentInfo.physicalResistance.HasValue;
        if (physDefGroup != null) physDefGroup.SetActive(showPhysDef);
        if (showPhysDef && physDefText != null)
            physDefText.text = $"{Mathf.RoundToInt(currentInfo.physicalResistance.Value * 100)}%";

        bool showMagicDef = currentInfo.magicalResistance.HasValue;
        if (magicDefGroup != null) magicDefGroup.SetActive(showMagicDef);
        if (showMagicDef && magicDefText != null)
            magicDefText.text = $"{Mathf.RoundToInt(currentInfo.magicalResistance.Value * 100)}%";

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        currentInfo = null;
        providerObject = null;
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!gameObject.activeSelf) return;

        // Provider が破棄されていたら閉じる（敵が倒れた、タワーが売却された等）
        if (providerObject == null)
        {
            Hide();
            return;
        }

        // リアルタイム HP 更新
        if (currentInfo?.getCurrentHp != null && hpGroup != null && hpGroup.activeSelf && hpText != null)
            hpText.text = $"{currentInfo.getCurrentHp()} / {currentInfo.maxHp}";

        // パネル外クリックで閉じる（同フレームで Show が呼ばれた直後はスキップ）
        if (justShown)
        {
            justShown = false;
            return;
        }

        var pointer = Pointer.current;
        if (pointer != null && pointer.press.wasPressedThisFrame)
        {
            Vector2 screenPos = pointer.position.ReadValue();
            Camera uiCamera = null;
            if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                uiCamera = canvas.worldCamera;

            if (panelRect != null &&
                !RectTransformUtility.RectangleContainsScreenPoint(panelRect, screenPos, uiCamera))
            {
                Hide();
            }
        }
    }
}
