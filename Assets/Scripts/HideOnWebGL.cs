using UnityEngine;

/// <summary>
/// WebGL ビルドでのみ、この GameObject を非表示にする。
/// Application.Quit() がブラウザでは機能しないため、Exit ボタンなどにアタッチして使う。
/// </summary>
public class HideOnWebGL : MonoBehaviour
{
    private void Awake()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        gameObject.SetActive(false);
#endif
    }
}
