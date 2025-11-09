using UnityEngine;
using UnityEngine.Tilemaps;

public class TowerSelectPanel : MonoBehaviour
{
    public static TowerSelectPanel Instance;
    private Tilemap frontTilemap;
    private Vector3 currentSpawnPosition;

    [SerializeField] private GameObject bowTowerPrefab;
    [SerializeField] private GameObject magicTowerPrefab;
    [SerializeField] private GameObject cannonTowerPrefab;

    void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);

        // frontTilemapを取得
        var frontOjb = GameObject.Find("Front");
        if (frontOjb != null)
        {
            frontTilemap = frontOjb.GetComponent<Tilemap>();
        }
    }

    public void Show(Vector3 spawnPosition)
    {
        currentSpawnPosition = spawnPosition;
        transform.position = Camera.main.WorldToScreenPoint(spawnPosition);
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    // ボタンから呼び出す
    public void OnSelectBow() => PlaceTower(bowTowerPrefab);
    public void OnSelectMagic() => PlaceTower(magicTowerPrefab);
    public void OnSelectCannon() => PlaceTower(cannonTowerPrefab);

    private void PlaceTower(GameObject prefab)
    {
        Instantiate(prefab, currentSpawnPosition, Quaternion.identity);
        Hide();

        // Front　Tilemapからタイルを消す
        if (frontTilemap != null)
        {
            frontTilemap.SetTile(frontTilemap.WorldToCell(currentSpawnPosition), null);
        }
    }
}