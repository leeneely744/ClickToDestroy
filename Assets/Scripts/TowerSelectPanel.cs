using TMPro;
using UnityEngine;

public class TowerSelectPanel : MonoBehaviour
{
    public static TowerSelectPanel Instance;
    private Vector3 currentSpawnPosition;
    private TowerPlace currentTowerPlace;
    private Money moneyController;

    [SerializeField] private GameObject bowTowerPrefab;
    [SerializeField] private GameObject magicTowerPrefab;
    [SerializeField] private GameObject cannonTowerPrefab;
    [SerializeField] private GameObject guardianTowerPrefab;

    [SerializeField] private TextMeshProUGUI bowCostText;
    [SerializeField] private TextMeshProUGUI magicCostText;
    [SerializeField] private TextMeshProUGUI cannonCostText;
    [SerializeField] private TextMeshProUGUI guardianCostText;

    void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);

        moneyController = FindAnyObjectByType<Money>();
        if (moneyController == null)
        {
            Debug.LogError("Money component not found. Please place Money UI (with Money.cs) in the scene.");
        }

        SetCostText(bowCostText, bowTowerPrefab);
        SetCostText(magicCostText, magicTowerPrefab);
        SetCostText(cannonCostText, cannonTowerPrefab);
        SetCostText(guardianCostText, guardianTowerPrefab);
    }

    private void SetCostText(TextMeshProUGUI text, GameObject prefab)
    {
        if (text == null || prefab == null) return;
        var controller = prefab.GetComponent<TowerController>();
        if (controller == null) return;
        text.text = controller.GetBuildCost().ToString();
    }

    public void Show(TowerPlace towerPlace)
    {
        currentTowerPlace = towerPlace;
        currentSpawnPosition = towerPlace.transform.position;
        transform.position = Camera.main.WorldToScreenPoint(currentSpawnPosition);
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        currentTowerPlace = null;
        gameObject.SetActive(false);
    }

    // ボタンから呼び出す
    public void OnSelectBow() => PlaceTower(bowTowerPrefab);
    public void OnSelectMagic() => PlaceTower(magicTowerPrefab);
    public void OnSelectCannon() => PlaceTower(cannonTowerPrefab);
    public void OnSelectGuardian() => PlaceTower(guardianTowerPrefab);

    private void PlaceTower(GameObject prefab)
    {
        var towerComponent = prefab.GetComponent<TowerController>();
        if (towerComponent == null)
        {
            Debug.LogError("Tower prefab does not contain TowerController component.");
            return;
        }

        int towerCost = towerComponent.GetBuildCost();

        if (moneyController == null || !moneyController.SpendMoney(towerCost))
        {
            Debug.Log("Not enough money to place tower.");
            return;
        }

        var newTowerObject = Instantiate(prefab, currentSpawnPosition, Quaternion.identity);
        if (currentTowerPlace != null)
        {
            currentTowerPlace.SetOccupied(true);
            newTowerObject.GetComponent<TowerController>().SetTowerPlace(currentTowerPlace);
        }
        Hide();
    }
}
