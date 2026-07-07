using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TowerActionPanel : MonoBehaviour
{
    public static TowerActionPanel Instance;
    public GameObject nextLevelPrefab;
    private TowerController towerController;

    [Header("Skill Purchase Confirmation")]
    [Tooltip("スキルボタンが確認状態のときに表示するスプライト")]
    [SerializeField] private Sprite confirmSprite;

    // 確認状態のスキルボタン index。-1 = 通常状態
    private int pendingConfirmIndex = -1;

    private GameObject[] skillButtons;
    private TMPro.TextMeshProUGUI[] skillNameTexts;
    private TMPro.TextMeshProUGUI[] skillCostTexts;
    private Image[] skillButtonImages;
    private Sprite[] normalSprites;

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

        skillButtons = new[] { skill1Button, skill2Button, skill3Button };
        skillNameTexts = new[] { skill1NameText, skill2NameText, skill3NameText };
        skillCostTexts = new[] { skill1CostText, skill2CostText, skill3CostText };
        skillButtonImages = new Image[skillButtons.Length];
        normalSprites = new Sprite[skillButtons.Length];
        for (int i = 0; i < skillButtons.Length; i++)
        {
            if (skillButtons[i] == null) continue;
            skillButtonImages[i] = skillButtons[i].GetComponent<Image>();
            if (skillButtonImages[i] == null)
            {
                Debug.LogWarning($"[TowerActionPanel] skill{i + 1}Button に Image がありません。確認スプライトの切替ができません。", this);
                continue;
            }
            normalSprites[i] = skillButtonImages[i].sprite;
        }

        gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        // StatusPanel が外クリック等で閉じられたら確認状態も解除する。
        // EventSystem のクリック処理より後に走る LateUpdate で判定することで、
        // 同一フレームの確認クリック（購入）を誤ってリセットしない。
        if (pendingConfirmIndex >= 0 && StatusPanel.Instance != null && !StatusPanel.Instance.IsVisible)
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
        for (int i = 0; i < skillButtons.Length; i++)
        {
            RefreshSkillButton(skillButtons[i], skillNameTexts[i], skillCostTexts[i], skills.Length > i ? skills[i] : null);
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
        towerController.UpgradeTower();
        Hide();
    }

    public void OnSellClick()
    {
        if (towerController == null)
        {
            Debug.LogError("TowerController not found");
            return;
        }
        towerController.SellTower();
        Hide();
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
            EnterConfirmState(index, skills[index]);
        }
    }

    private void EnterConfirmState(int index, IPurchasableSkill skill)
    {
        // 別のボタンが確認状態なら先に戻す
        ResetConfirmState();
        pendingConfirmIndex = index;

        if (confirmSprite == null)
        {
            Debug.LogWarning("[TowerActionPanel] confirmSprite が設定されていません。Inspector で確認ボタン用スプライトを割り当ててください。", this);
        }
        else if (skillButtonImages[index] != null)
        {
            skillButtonImages[index].sprite = confirmSprite;
        }

        if (StatusPanel.Instance == null)
        {
            Debug.LogWarning("[TowerActionPanel] StatusPanel がシーンに存在しません。スキル説明を表示できません。", this);
            return;
        }
        StatusPanel.Instance.ShowSkillInfo(skill);
    }

    private void ConfirmPurchase(int index)
    {
        ResetConfirmState();

        if (!towerController.TryPurchaseSkill(index))
        {
            // 資金不足など。確認状態は解除済みなので通常状態に戻るだけ
            return;
        }

        skillButtons[index].SetActive(false);
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
        if (skillButtonImages != null && skillButtonImages[index] != null && normalSprites[index] != null)
        {
            skillButtonImages[index].sprite = normalSprites[index];
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
