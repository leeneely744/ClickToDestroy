using UnityEngine;
using UnityEngine.Tilemaps;

public class TowerSelectPanel : MonoBehaviour
{
    public static TowerSelectPanel Instance;
    private Vector3 currentSpawnPosition;
    private TowerPlace currentTowerPlace;
    private Money moneyController;

    [SerializeField] private GameObject bowTowerPrefab;
    [SerializeField] private GameObject magicTowerPrefab;
    [SerializeField] private GameObject cannonTowerPrefab;

    void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);

        moneyController = FindObjectOfType<Money>();
        if (moneyController == null)
        {
            Debug.LogError("Money component not found. Please place Money UI (with Money.cs) in the scene.");
        }
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

    private void PlaceTower(GameObject prefab)
    {
        int nowMoney = moneyController.money;
        int towerCost = prefab.GetComponent<TowerController>().cost;

        if (nowMoney < towerCost)
        {
            Debug.Log("Not enough money to place tower.");
            return;
        }
        moneyController.SpendMoney(towerCost);

        Instantiate(prefab, currentSpawnPosition, Quaternion.identity);
        if (currentTowerPlace != null)
        {
            currentTowerPlace.SetOccupied(true);
        }
        Hide();
    }
}
