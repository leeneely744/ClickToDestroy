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
    private TowerFusionService fusionService;
    private FusionManager fusionManager;
    private Vector3 initialPosition;

    private void Awake()
    {
        tower = GetComponent<TowerController>();
        if (tower == null)
        {
            Debug.LogWarning($"[DragController] TowerController not found on {name}. This object will still log drag events, but fusion logic will not work.");
        }

        fusionService = new TowerFusionService();
        fusionManager = FusionManager.Instance;
        if (fusionManager == null)
        {
            Debug.LogError("[DragController] FusionManager instance not found in the scene. Fusion logic will not work.");
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // ドラッグしたタワーを元の位置に戻すための、最初のポジションを取得
        initialPosition = transform.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        var position = eventData.position;

        if (Camera.main == null)
        {
            Debug.LogError("[DragController] OnEndDrag: Camera.main is null. Cannot raycast to find target tower.");
            transform.position = initialPosition;
            return;
        }

        if (tower == null)
        {
            Debug.LogError("[DragController] OnEndDrag: TowerController is null. Cannot perform fusion.");
            transform.position = initialPosition;
            return;
        }

        if (fusionManager == null)
        {
            // Awake でエラーを出しているが、念のためここでもチェック
            fusionManager = FusionManager.Instance;
            if (fusionManager == null)
            {
                Debug.LogError("[DragController] OnEndDrag: FusionManager.Instance is still null. Fusion cannot be attempted.");
                transform.position = initialPosition;
                return;
            }
        }

        // 画面座標 → ワールド座標（2D 用）。Z は 0 に固定。
        var worldPos = Camera.main.ScreenToWorldPoint(position);
        worldPos.z = 0f;

        // 2D コライダーを想定して、Raycast でヒットを確認
        var hit2D = Physics2D.Raycast(worldPos, Vector2.zero);
        if (hit2D.collider == null)
        {
            transform.position = initialPosition;
            return;
        }

        // コライダーが付いているのは多くの場合タワーの子オブジェクト（例: AttackRangeCircle）なので、
        // 親階層までさかのぼって TowerController を探す。
        var targetTower = hit2D.collider.GetComponentInParent<TowerController>();
        if (targetTower == null)
        {
            transform.position = initialPosition;
            return;
        }

        if (targetTower == tower)
        {
            transform.position = initialPosition;
            return;
        }

        bool success = fusionManager.TryFuse(tower, targetTower);

        if (!success)
        {
            transform.position = initialPosition;
        }
    }
}
