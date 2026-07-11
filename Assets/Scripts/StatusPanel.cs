using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

// ======================================================
// Unity エディタでの UI セットアップ手順:
//
//   StatusPanel (このスクリプト, Image, anchored bottom)
//     ├── IconImage   : Image
//     ├── NameText    : TextMeshProUGUI
//     ├── HpText      : TextMeshProUGUI
//     ├── AttackText  : TextMeshProUGUI
//     ├── PhysDefText : TextMeshProUGUI
//     └── MagicDefText: TextMeshProUGUI
//
// グループ不要。全て StatusPanel の直下に並べる。
// ======================================================
public class StatusPanel : MonoBehaviour
{
    public static StatusPanel Instance { get; private set; }

    [Header("Panel")]
    [SerializeField] private RectTransform panelRect;

    [Header("Header")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;

    [Header("Stats")]
    [SerializeField] private TMP_Text hpText;
    [SerializeField] private TMP_Text attackText;
    [SerializeField] private TMP_Text physDefText;
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
        if (hpText != null)
        {
            hpText.gameObject.SetActive(showHp);
            if (showHp) hpText.text = $"{currentInfo.getCurrentHp()} / {currentInfo.maxHp}";
        }

        bool showAtk = currentInfo.attackDamage.HasValue;
        if (attackText != null)
        {
            attackText.gameObject.SetActive(showAtk);
            if (showAtk) attackText.text = $"攻撃力：{currentInfo.attackDamage}";
        }

        bool showPhysDef = currentInfo.physicalResistance.HasValue;
        if (physDefText != null)
        {
            physDefText.gameObject.SetActive(showPhysDef);
            if (showPhysDef)
            {
                int pct = Mathf.RoundToInt(currentInfo.physicalResistance.Value * 100);
                physDefText.text = pct == 0 ? "防御：なし" : $"防御：{pct}%";
            }
        }

        bool showMagicDef = currentInfo.magicalResistance.HasValue;
        if (magicDefText != null)
        {
            magicDefText.gameObject.SetActive(showMagicDef);
            if (showMagicDef)
            {
                int pct = Mathf.RoundToInt(currentInfo.magicalResistance.Value * 100);
                magicDefText.text = pct == 0 ? "魔防：なし" : $"魔防：{pct}%";
            }
        }

        gameObject.SetActive(true);
    }

    /// <summary>
    /// 任意のテキストだけを NameText に表示するモード（購入・売却などの確認表示用）。
    /// ステータス系の行（HP・攻撃・防御）とアイコンは非表示にする。
    /// owner が破棄されるとパネルは自動で閉じる。
    /// </summary>
    public void ShowText(string message, Object owner)
    {
        currentInfo = null;
        providerObject = owner;
        justShown = true;

        if (iconImage != null)
            iconImage.enabled = false;

        if (nameText != null)
            nameText.text = message;

        if (hpText != null) hpText.gameObject.SetActive(false);
        if (attackText != null) attackText.gameObject.SetActive(false);
        if (physDefText != null) physDefText.gameObject.SetActive(false);
        if (magicDefText != null) magicDefText.gameObject.SetActive(false);

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

        if (providerObject == null)
        {
            Hide();
            return;
        }

        // リアルタイム HP 更新
        if (currentInfo?.getCurrentHp != null && hpText != null && hpText.gameObject.activeSelf)
            hpText.text = $"{currentInfo.getCurrentHp()} / {currentInfo.maxHp}";

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
