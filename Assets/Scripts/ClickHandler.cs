using UnityEngine;
using UnityEngine.EventSystems;

public class ClickHandler : MonoBehaviour, IPointerClickHandler
{
    private TowerController parentTower;

    void Start()
    {
        parentTower = transform.parent.GetComponent<TowerController>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (parentTower != null)
        {
            parentTower.OnSelected();
        }
    }
}