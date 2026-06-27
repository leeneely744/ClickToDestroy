using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteFrameAnimator : MonoBehaviour
{
    [SerializeField] private Sprite[] frames;
    [SerializeField] private float fps = 12f;
    [SerializeField] private bool loop = false;

    private void Start()
    {
        if (frames == null || frames.Length == 0)
        {
            if (!loop) Destroy(gameObject);
            return;
        }
        StartCoroutine(Play());
    }

    private IEnumerator Play()
    {
        var spriteRenderer = GetComponent<SpriteRenderer>();
        float interval = 1f / fps;

        do
        {
            foreach (var frame in frames)
            {
                spriteRenderer.sprite = frame;
                yield return new WaitForSeconds(interval);
            }
        } while (loop);

        Destroy(gameObject);
    }
}
