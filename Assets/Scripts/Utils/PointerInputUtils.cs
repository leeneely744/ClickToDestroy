using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// マウス入力まわりの共通処理。Hero / Guardian の移動先指定などで使う。
/// </summary>
public static class PointerInputUtils
{
    private static readonly List<RaycastResult> raycastResults = new List<RaycastResult>();

    /// <summary>このフレームで左クリックが押されたか。</summary>
    public static bool WasLeftClickPressedThisFrame()
    {
        return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
    }

    /// <summary>
    /// ポインタが UI（GraphicRaycaster）上にあるか。
    /// Physics2DRaycaster も IsPointerOverGameObject に含まれるため、GraphicRaycaster のみを判定する。
    /// </summary>
    public static bool IsPointerOverUI()
    {
        if (EventSystem.current == null || Mouse.current == null) return false;

        var pointer = new PointerEventData(EventSystem.current)
        {
            position = Mouse.current.position.ReadValue()
        };
        raycastResults.Clear();
        EventSystem.current.RaycastAll(pointer, raycastResults);
        foreach (var result in raycastResults)
        {
            if (result.module is GraphicRaycaster) return true;
        }
        return false;
    }

    /// <summary>
    /// 現在のポインタ位置をワールド座標（z = 0）で取得する。
    /// Main Camera が見つからない場合は警告を出して false を返す。
    /// </summary>
    public static bool TryGetPointerWorldPosition(out Vector3 worldPosition, Object context = null)
    {
        worldPosition = Vector3.zero;

        if (Mouse.current == null) return false;

        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogWarning("[PointerInputUtils] Main Camera が見つかりません。", context);
            return false;
        }

        Vector2 screenPosition = Mouse.current.position.ReadValue();
        worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, mainCamera.nearClipPlane));
        worldPosition.z = 0f;
        return true;
    }
}
