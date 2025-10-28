using UnityEngine;
using UnityEngine.EventSystems;

public class ClickToDestroy : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"{gameObject.name} clicked and will be destroyed.");
        Destroy(gameObject);
    }
}