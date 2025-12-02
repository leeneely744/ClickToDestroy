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
}
