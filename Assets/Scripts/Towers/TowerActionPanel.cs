using UnityEngine;
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

    [Header("Rows")]
    [SerializeField] private GameObject rowTop;
    [SerializeField] private GameObject rowMiddle;
    [SerializeField] private GameObject rowBottom;

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

        Vector3 screenPos = Camera.main.WorldToScreenPoint(towerController.transform.position);
        transform.position = screenPos;
        gameObject.SetActive(true);

        if (upgradeCostText != null)
            upgradeCostText.text = towerController.GetUpgradeCost().ToString();

        if (sellRefundText != null)
            sellRefundText.text = towerController.GetSellValue().ToString();

        if (moveGuardianButton != null)
            moveGuardianButton.SetActive(towerController is GuardianTowerControllerBase);

        var skills = towerController.GetSkills();
        RefreshSkillButton(skill1Button, skill1CostText, skills.Length > 0 ? skills[0] : null);
        RefreshSkillButton(skill2Button, skill2CostText, skills.Length > 1 ? skills[1] : null);
        RefreshSkillButton(skill3Button, skill3CostText, skills.Length > 2 ? skills[2] : null);

        RefreshRow(rowTop);
        RefreshRow(rowMiddle);
        RefreshRow(rowBottom);
    }

    private void RefreshSkillButton(GameObject button, TMPro.TextMeshProUGUI costText, IPurchasableSkill skill)
    {
        if (button == null) return;
        if (skill == null || skill.IsPurchased)
        {
            button.SetActive(false);
            return;
        }
        button.SetActive(true);
        Debug.Log($"[RefreshSkillButton] button={button.name}, costText={(costText != null ? costText.name : "null")}, Cost={skill.Cost}");
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

    public void OnSkill1Click()
    {
        if (towerController == null) return;
        if (towerController.TryPurchaseSkill(0))
        {
            skill1Button.SetActive(false);
            RefreshRow(rowTop);
        }
    }

    public void OnSkill2Click()
    {
        if (towerController == null) return;
        if (towerController.TryPurchaseSkill(1))
        {
            skill2Button.SetActive(false);
            RefreshRow(rowTop);
        }
    }

    public void OnSkill3Click()
    {
        if (towerController == null) return;
        if (towerController.TryPurchaseSkill(2))
        {
            skill3Button.SetActive(false);
            RefreshRow(rowMiddle);
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
