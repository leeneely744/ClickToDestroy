using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// タワーをドラッグ＆ドロップ可能にするための最小限のドラッグコントローラー。
/// 現時点では、ドラッグ開始／ドラッグ中／ドラッグ終了のイベントを受け取り、
/// Console にログを出すだけの実装にしておく。
/// 実際の合成処理や座標移動は、後のステップで追加する。
/// </summary>
public class DragController : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private TowerController tower;

    private void Awake()
    {
        tower = GetComponent<TowerController>();
        if (tower == null)
        {
            Debug.LogWarning($"[DragController] TowerController not found on {name}. This object will still log drag events, but fusion logic will not work.");
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log($"[DragController] OnBeginDrag: {name}, pointer={eventData.pointerId}, position={eventData.position}");
    }

    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log($"[DragController] OnDrag: {name}, position={eventData.position}");
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log($"[DragController] OnEndDrag: {name}, position={eventData.position}");
    }
}

