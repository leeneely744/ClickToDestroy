using UnityEngine;
using UnityEngine.Tilemaps;

public class TowerActionPanel : MonoBehaviour
{
    public static TowerActionPanel Instance;
    private TowerController towerController;

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
    }

    public void Hide()
    {
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
