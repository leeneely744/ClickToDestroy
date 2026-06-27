using UnityEngine;

public class FusionManager : MonoBehaviour
{
    public static FusionManager Instance { get; private set; }

    private TowerFusionService fusionService;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        fusionService = new TowerFusionService();
        fusionService.LoadFromResources("TowerFusion");
    }

    public bool CanFuse(TowerController a, TowerController b, out TowerFusionRecipe recipe)
    {
        return fusionService.CanFuse(a, b, out recipe);
    }

    public bool TryFuse(TowerController source, TowerController target)
    {
        if (!fusionService.CanFuse(source, target, out TowerFusionRecipe recipe))
        {
            return false;
        }

        var targetPlace = target.GetTowerPlace();
        if (targetPlace == null)
        {
            Debug.LogWarning("[FusionManager] Target tower has no TowerPlace. Fusion aborted.");
            return false;
        }

        if (recipe.resultTowerPrefab == null)
        {
            Debug.LogError("[FusionManager] Fusion recipe has no resultTowerPrefab.");
            return false;
        }

        source.DestroyTower();
        target.DestroyTower();

        var newTowerObj = Instantiate(recipe.resultTowerPrefab, targetPlace.transform.position, Quaternion.identity);
        var newController = newTowerObj.GetComponent<TowerController>();
        if (newController != null)
        {
            newController.SetTowerPlace(targetPlace);
        }
        else
        {
            Debug.LogError("[FusionManager] Result tower prefab does not have a TowerController.");
        }

        targetPlace.SetOccupied(true);

        return true;
    }
}
