using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class TowerActionPanel : MonoBehaviour
{
    public static TowerActionPanel Instance;
    public GameObject nextLevelPrefab;
    private TowerController towerController;

    [Header("Purchase Confirmation")]
    [Tooltip("ボタンが確認状態のときに表示するスプライト")]
    [SerializeField] private Sprite confirmSprite;
    [Tooltip("売却ボタンの GameObject（確認スプライト切替用）")]
    [SerializeField] private GameObject sellButton;

    // 確認状態のボタン index。0〜2 = スキル、3 = アップグレード、4 = 売却、-1 = 通常状態
    private const int SkillCount = 3;
    private const int UpgradeIndex = 3;
    private const int SellIndex = 4;
    private int pendingConfirmIndex = -1;

    private GameObject[] confirmButtons;
    private TMPro.TextMeshProUGUI[] skillNameTexts;
    private TMPro.TextMeshProUGUI[] skillCostTexts;
    private Image[] confirmButtonImages;
    private Sprite[] normalSprites;
    private Canvas parentCanvas;

    [SerializeField] private GameObject upgradeButton;
    [SerializeField] private TMPro.TextMeshProUGUI upgradeCostText;
    [SerializeField] private TMPro.TextMeshProUGUI sellRefundText;
    [SerializeField] private GameObject moveGuardianButton;
    [SerializeField] private GameObject skill1Button;
    [SerializeField] private TMPro.TextMeshProUGUI skill1NameText;
    [SerializeField] private TMPro.TextMeshProUGUI skill1CostText;
    [SerializeField] private GameObject skill2Button;
    [SerializeField] private TMPro.TextMeshProUGUI skill2NameText;
    [SerializeField] private TMPro.TextMeshProUGUI skill2CostText;
    [SerializeField] private GameObject skill3Button;
    [SerializeField] private TMPro.TextMeshProUGUI skill3NameText;
    [SerializeField] private TMPro.TextMeshProUGUI skill3CostText;

    [Header("Rows")]
    [SerializeField] private GameObject rowTop;
    [SerializeField] private GameObject rowMiddle;
    [SerializeField] private GameObject rowBottom;

    void Awake()
    {
        Instance = this;

        confirmButtons = new[] { skill1Button, skill2Button, skill3Button, upgradeButton, sellButton };
        skillNameTexts = new[] { skill1NameText, skill2NameText, skill3NameText };
        skillCostTexts = new[] { skill1CostText, skill2CostText, skill3CostText };
        confirmButtonImages = new Image[confirmButtons.Length];
        normalSprites = new Sprite[confirmButtons.Length];
        string[] buttonNames = { "skill1Button", "skill2Button", "skill3Button", "upgradeButton", "sellButton" };
        for (int i = 0; i < confirmButtons.Length; i++)
        {
            if (confirmButtons[i] == null)
            {
                Debug.LogWarning($"[TowerActionPanel] {buttonNames[i]} が設定されていません。Inspector を確認してください。", this);
                continue;
            }
            confirmButtonImages[i] = confirmButtons[i].GetComponent<Image>();
            if (confirmButtonImages[i] == null)
            {
                Debug.LogWarning($"[TowerActionPanel] {buttonNames[i]} に Image がありません。確認スプライトの切替ができません。", this);
                continue;
            }
            normalSprites[i] = confirmButtonImages[i].sprite;
        }

        parentCanvas = GetComponentInParent<Canvas>();

        gameObject.SetActive(false);
    }

    private void Update()
    {
        // 確認状態のとき、確認ボタンの外を「押した」瞬間にキャンセルする。
        // Button.onClick はマウスを離した瞬間に発火するため、
        // 「押した位置がボタン内なら何もしない」ことで確認クリックと競合しない。
        if (pendingConfirmIndex < 0) return;

        var pointer = Pointer.current;
        if (pointer == null || !pointer.press.wasPressedThisFrame) return;

        var buttonObj = confirmButtons[pendingConfirmIndex];
        if (buttonObj == null || buttonObj.transform is not RectTransform buttonRect)
        {
            ResetConfirmState();
            return;
        }

        Vector2 screenPos = pointer.position.ReadValue();
        Camera uiCamera = parentCanvas != null && parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? parentCanvas.worldCamera
            : null;

        if (!RectTransformUtility.RectangleContainsScreenPoint(buttonRect, screenPos, uiCamera))
        {
            ResetConfirmState();
        }
    }

    public void Show(TowerController controller)
    {
        ResetConfirmState();

        towerController = controller;
        if (towerController == null)
        {
            Debug.LogError("TowerController not found in parent objects");
            return;
        }

        Vector3 screenPos = Camera.main.WorldToScreenPoint(towerController.transform.position);
        transform.position = screenPos;
        gameObject.SetActive(true);

        bool canUpgrade = towerController.NextLevelPrefab != null;
        if (upgradeButton != null)
            upgradeButton.SetActive(canUpgrade);
        if (upgradeCostText != null)
            upgradeCostText.text = canUpgrade ? towerController.GetUpgradeCost().ToString() : "";

        if (sellRefundText != null)
            sellRefundText.text = towerController.GetSellValue().ToString();

        if (moveGuardianButton != null)
            moveGuardianButton.SetActive(towerController is GuardianTowerControllerBase);

        var skills = towerController.GetSkills();
        bool hasUnpurchasedSkill = false;
        for (int i = 0; i < SkillCount; i++)
        {
            var skill = skills.Length > i ? skills[i] : null;
            RefreshSkillButton(confirmButtons[i], skillNameTexts[i], skillCostTexts[i], skill);
            if (skill != null && !skill.IsPurchased)
            {
                hasUnpurchasedSkill = true;
            }
        }

        // 購入可能なスキルを初めて目にしたときのヒント
        if (hasUnpurchasedSkill)
        {
            TutorialHintService.TryShow(TutorialHints.Skill);
        }

        RefreshRow(rowTop);
        RefreshRow(rowMiddle);
        RefreshRow(rowBottom);
    }

    private void RefreshSkillButton(GameObject button, TMPro.TextMeshProUGUI nameText, TMPro.TextMeshProUGUI costText, IPurchasableSkill skill)
    {
        if (button == null) return;
        if (skill == null || skill.IsPurchased)
        {
            button.SetActive(false);
            return;
        }
        button.SetActive(true);
        if (nameText != null) nameText.text = skill.SkillName;
        if (costText != null) costText.text = skill.Cost.ToString();
    }

    // 子に activeSelf=true のものがなければ段ごと非表示にする
    private void RefreshRow(GameObject row)
    {
        if (row == null) return;
        foreach (Transform child in row.transform)
        {
            if (child.gameObject.activeSelf)
            {
                row.SetActive(true);
                return;
            }
        }
        row.SetActive(false);
    }

    public void Hide()
    {
        ResetConfirmState();

        if (towerController == null)
        {
            Debug.LogError("TowerController not found");
            return;
        }
        towerController.TurnOffAttackRange();
        towerController = null;
        gameObject.SetActive(false);
    }

    public void OnUpgradeClick()
    {
        if (towerController == null)
        {
            Debug.LogError("TowerController not found");
            return;
        }

        if (pendingConfirmIndex == UpgradeIndex)
        {
            ResetConfirmState();
            towerController.UpgradeTower();
            if (StatusPanel.Instance != null) StatusPanel.Instance.Hide();
            Hide();
        }
        else
        {
            EnterConfirmState(UpgradeIndex, $"アップグレード：{towerController.GetUpgradeCost()}");
        }
    }

    public void OnSellClick()
    {
        if (towerController == null)
        {
            Debug.LogError("TowerController not found");
            return;
        }

        if (pendingConfirmIndex == SellIndex)
        {
            ResetConfirmState();
            towerController.SellTower();
            if (StatusPanel.Instance != null) StatusPanel.Instance.Hide();
            Hide();
        }
        else
        {
            EnterConfirmState(SellIndex, $"売却：{towerController.GetSellValue()}");
        }
    }

    // Button の onClick には従来どおりこの 3 メソッドを割り当てる
    public void OnSkill1Click() => OnSkillButtonClick(0);
    public void OnSkill2Click() => OnSkillButtonClick(1);
    public void OnSkill3Click() => OnSkillButtonClick(2);

    /// <summary>
    /// スキルボタンの 2 段階購入フロー。
    /// 1 クリック目: ボタンを確認状態にし、StatusPanel にスキルの説明を表示する。
    /// 2 クリック目（確認状態のボタン）: 購入を実行する。
    /// </summary>
    private void OnSkillButtonClick(int index)
    {
        if (towerController == null) return;

        var skills = towerController.GetSkills();
        if (index < 0 || index >= skills.Length) return;

        if (pendingConfirmIndex == index)
        {
            ConfirmPurchase(index);
        }
        else
        {
            EnterConfirmState(index, skills[index].SkillName);
        }
    }

    /// <summary>
    /// 指定ボタンを確認状態にし、StatusPanel に確認メッセージを表示する。
    /// </summary>
    private void EnterConfirmState(int index, string message)
    {
        // 別のボタンが確認状態なら先に戻す
        ResetConfirmState();
        pendingConfirmIndex = index;

        if (confirmSprite == null)
        {
            Debug.LogWarning("[TowerActionPanel] confirmSprite が設定されていません。Inspector で確認ボタン用スプライトを割り当ててください。", this);
        }
        else if (confirmButtonImages[index] != null)
        {
            confirmButtonImages[index].sprite = confirmSprite;
        }

        if (StatusPanel.Instance == null)
        {
            Debug.LogWarning("[TowerActionPanel] StatusPanel がシーンに存在しません。確認メッセージを表示できません。", this);
            return;
        }
        StatusPanel.Instance.ShowText(message, towerController);
    }

    private void ConfirmPurchase(int index)
    {
        ResetConfirmState();

        if (!towerController.TryPurchaseSkill(index))
        {
            // 資金不足など。確認状態は解除済みなので通常状態に戻るだけ
            return;
        }

        confirmButtons[index].SetActive(false);
        RefreshRow(index == 2 ? rowMiddle : rowTop);

        if (StatusPanel.Instance != null)
        {
            StatusPanel.Instance.Hide();
        }
    }

    private void ResetConfirmState()
    {
        if (pendingConfirmIndex < 0) return;

        int index = pendingConfirmIndex;
        pendingConfirmIndex = -1;
        if (confirmButtonImages != null && confirmButtonImages[index] != null && normalSprites[index] != null)
        {
            confirmButtonImages[index].sprite = normalSprites[index];
        }
    }

    public void OnMoveGuardiansClick()
    {
        if (towerController == null)
        {
            Debug.LogError("TowerController not found");
            return;
        }
        if (towerController is GuardianTowerControllerBase guardianTower)
        {
            guardianTower.StartMoveMode();
            Hide();
        }
        else
        {
            Debug.LogWarning("MoveGuardian button pressed on a non-guardian tower.");
        }
    }
}
