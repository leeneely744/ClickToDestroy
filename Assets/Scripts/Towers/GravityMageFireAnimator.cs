using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class GravityMageFireAnimator : MonoBehaviour
{
    [SerializeField] private Sprite[] frames;
    [SerializeField] private float fps = 10f;

    private SpriteRenderer sr;

    private void Awake() { sr = GetComponent<SpriteRenderer>(); }

    private void Start() { StartCoroutine(Loop()); }

    private IEnumerator Loop()
    {
        if (frames == null || frames.Length == 0) yield break;
        float interval = 1f / fps;
        int i = 0;
        while (true)
        {
            sr.sprite = frames[i % frames.Length];
            i++;
            yield return new WaitForSeconds(interval);
        }
    }
}
