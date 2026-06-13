using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;

public class TowerActionPanel : MonoBehaviour
{
    public static TowerActionPanel Instance;
    public GameObject nextLevelPrefab;
    private TowerController towerController;
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

    void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);
    }

    public void Show(TowerController controller)
    {
        towerController = controller;
        if (towerController == null)
        {
            Debug.LogError("TowerController not found in parent objects");
            return;
        }

        // UIの中心を塔の位置に合わせる
        Vector3 screenPos = Camera.main.WorldToScreenPoint(towerController.transform.position);
        transform.position = screenPos;
        gameObject.SetActive(true);

        // アップグレード金額表示
        // TODO: 次のレベルのプレハブがない場合はアップグレード不可にする
        int upgradeCost = towerController.GetUpgradeCost();
        if (upgradeCostText != null)
        {
            upgradeCostText.text = upgradeCost.ToString();
        }

        // 売却金額表示
        int sellRefund = towerController.GetSellValue();
        if (sellRefundText != null)
        {
            sellRefundText.text = sellRefund.ToString();
        }

        // 衛兵移動ボタン表示
        if (moveGuardianButton != null)
        {
            bool canMoveGuardians = towerController is GuardianTowerControllerBase;
            moveGuardianButton.SetActive(canMoveGuardians);
        }

        // スキルボタン表示
        var skills = towerController.GetSkills();
        RefreshSkillButton(skill1Button, skill1NameText, skill1CostText, skills.Length > 0 ? skills[0] : null);
        RefreshSkillButton(skill2Button, skill2NameText, skill2CostText, skills.Length > 1 ? skills[1] : null);
        RefreshSkillButton(skill3Button, skill3NameText, skill3CostText, skills.Length > 2 ? skills[2] : null);
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
        if (costText != null) costText.text = skill.Cost.ToString();
    }

    public void Hide()
    {
        if (towerController == null)
        {
            Debug.LogError("TowerController not found");
            return;
        }
        // 必ず攻撃範囲表示をオフにしてから towerController をクリアする
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
        gameObject.SetActive(false);

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
        gameObject.SetActive(false);

        Hide();
    }

    public void OnSkill1Click()
    {
        if (towerController == null) return;
        if (towerController.TryPurchaseSkill(0))
            RefreshSkillButton(skill1Button, skill1NameText, skill1CostText, null);
    }

    public void OnSkill2Click()
    {
        if (towerController == null) return;
        if (towerController.TryPurchaseSkill(1))
            RefreshSkillButton(skill2Button, skill2NameText, skill2CostText, null);
    }

    public void OnSkill3Click()
    {
        if (towerController == null) return;
        if (towerController.TryPurchaseSkill(2))
            RefreshSkillButton(skill3Button, skill3NameText, skill3CostText, null);
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
            gameObject.SetActive(false);
            Hide();
        }
        else
        {
            Debug.LogWarning("MoveGuardian button pressed on a non-guardian tower.");
        }
    }
}
