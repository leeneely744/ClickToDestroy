using UnityEngine;
using UnityEngine.EventSystems;

public class DragController : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private TowerController tower;
    private FusionManager fusionManager;
    private Vector3 initialPosition;
    private GameObject ghostObject;
    private SpriteRenderer ghostSr;
    private Sprite originalGhostSprite;

    private TowerController hoveredTarget;
    private Color originalTargetColor;

    private static readonly Color FusionHighlightColor = new Color(0.3f, 1f, 0.3f, 1f);

    private void Awake()
    {
        tower = GetComponent<TowerController>();
        fusionManager = FusionManager.Instance;
    }

    public void OnBeginDrag(PointerEventData _)
    {
        initialPosition = transform.position;
        CreateGhost();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (ghostObject == null || Camera.main == null) return;
        var worldPos = Camera.main.ScreenToWorldPoint(eventData.position);
        worldPos.z = 0f;
        ghostObject.transform.position = worldPos;

        UpdateFusionHover(worldPos);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        ClearHover();
        DestroyGhost();

        if (Camera.main == null || tower == null) return;

        if (fusionManager == null)
        {
            fusionManager = FusionManager.Instance;
            if (fusionManager == null) return;
        }

        var worldPos = Camera.main.ScreenToWorldPoint(eventData.position);
        worldPos.z = 0f;

        var hit2D = Physics2D.Raycast(worldPos, Vector2.zero);
        if (hit2D.collider == null) return;

        var targetTower = hit2D.collider.GetComponentInParent<TowerController>();
        if (targetTower == null || targetTower == tower) return;

        fusionManager.TryFuse(tower, targetTower);
    }

    private void UpdateFusionHover(Vector3 worldPos)
    {
        if (fusionManager == null) return;

        TowerController newTarget = null;
        TowerFusionRecipe recipe = null;

        var hit2D = Physics2D.Raycast(worldPos, Vector2.zero);
        if (hit2D.collider != null)
        {
            var candidate = hit2D.collider.GetComponentInParent<TowerController>();
            if (candidate != null && candidate != tower && fusionManager.CanFuse(tower, candidate, out recipe))
                newTarget = candidate;
        }

        if (newTarget == hoveredTarget) return;

        ClearHover();

        if (newTarget != null)
        {
            hoveredTarget = newTarget;
            var sr = hoveredTarget.GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
            {
                originalTargetColor = sr.color;
                sr.color = FusionHighlightColor;
            }

            if (ghostSr != null && recipe?.resultTowerPrefab != null)
            {
                var resultSr = recipe.resultTowerPrefab.GetComponentInChildren<SpriteRenderer>();
                if (resultSr != null)
                    ghostSr.sprite = resultSr.sprite;
            }
        }
    }

    private void ClearHover()
    {
        if (hoveredTarget != null)
        {
            var sr = hoveredTarget.GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
                sr.color = originalTargetColor;
            hoveredTarget = null;
        }

        if (ghostSr != null && originalGhostSprite != null)
            ghostSr.sprite = originalGhostSprite;
    }

    private void CreateGhost()
    {
        ghostObject = new GameObject("DragGhost");

        var sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            ghostObject.transform.position = sr.transform.position;
            ghostObject.transform.localScale = sr.transform.lossyScale;

            ghostSr = ghostObject.AddComponent<SpriteRenderer>();
            ghostSr.sprite = sr.sprite;
            ghostSr.color = new Color(1f, 1f, 1f, 0.5f);
            ghostSr.sortingLayerID = sr.sortingLayerID;
            ghostSr.sortingOrder = sr.sortingOrder + 10;
            originalGhostSprite = sr.sprite;
        }
        else
        {
            ghostObject.transform.position = transform.position;
        }
    }

    private void DestroyGhost()
    {
        if (ghostObject != null)
        {
            Destroy(ghostObject);
            ghostObject = null;
            ghostSr = null;
            originalGhostSprite = null;
        }
    }
}
