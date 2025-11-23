using UnityEngine;
using UnityEngine.EventSystems;

public class TowerPlace : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private SpriteRenderer indicatorRenderer;

    private bool isOccupied;

    private void Awake()
    {
        if (indicatorRenderer == null)
        {
            indicatorRenderer = GetComponent<SpriteRenderer>();
        }
        UpdateIndicator();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isOccupied)
        {
            return;
        }

        TowerSelectPanel.Instance.Show(this);
    }

    public void SetOccupied(bool occupied)
    {
        isOccupied = occupied;
        UpdateIndicator();
    }

    private void UpdateIndicator()
    {
        if (indicatorRenderer != null)
        {
            indicatorRenderer.enabled = !isOccupied;
        }
    }
}
