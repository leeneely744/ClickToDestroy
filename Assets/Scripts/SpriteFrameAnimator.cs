using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteFrameAnimator : MonoBehaviour
{
    [SerializeField] private Sprite[] frames;
    [SerializeField] private float fps = 12f;

    private void Start()
    {
        if (frames == null || frames.Length == 0)
        {
            Destroy(gameObject);
            return;
        }
        StartCoroutine(Play());
    }

    private IEnumerator Play()
    {
        var renderer = GetComponent<SpriteRenderer>();
        float interval = 1f / fps;

        foreach (var frame in frames)
        {
            renderer.sprite = frame;
            yield return new WaitForSeconds(interval);
        }

        Destroy(gameObject);
    }
}
