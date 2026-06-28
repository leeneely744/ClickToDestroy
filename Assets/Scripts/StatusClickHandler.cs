using UnityEngine;
using UnityEngine.EventSystems;

// IStatusProvider を持つ親オブジェクトのステータスを StatusPanel に表示する。
// EnemyController / GuardianController / HeroController の Awake() で
// 子コライダーに動的にアタッチされる。
public class StatusClickHandler : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        var provider = GetComponentInParent<IStatusProvider>();
        if (provider != null)
            StatusPanel.Instance?.Show(provider);
    }
}
