using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ElfArcherSpriteAnimator : MonoBehaviour
{
    [SerializeField] private Sprite[] idleFrames;
    [SerializeField] private Sprite[] attackFrames;
    [SerializeField] private float fps = 12f;

    private SpriteRenderer sr;
    private bool isAttacking;
    private Coroutine idleCoroutine;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        isAttacking = false;
        idleCoroutine = StartCoroutine(IdleLoop());
    }

    private void OnDisable()
    {
        if (idleCoroutine != null)
        {
            StopCoroutine(idleCoroutine);
            idleCoroutine = null;
        }
    }

    private IEnumerator IdleLoop()
    {
        if (idleFrames == null || idleFrames.Length == 0) yield break;
        float interval = 1f / fps;
        int i = 0;
        while (true)
        {
            if (!isAttacking)
            {
                sr.sprite = idleFrames[i % idleFrames.Length];
                i++;
            }
            yield return new WaitForSeconds(interval);
        }
    }

    public void PlayAttack()
    {
        if (!isAttacking)
            StartCoroutine(AttackOnce());
    }

    private IEnumerator AttackOnce()
    {
        if (attackFrames == null || attackFrames.Length == 0) yield break;
        isAttacking = true;
        float interval = 1f / fps;
        foreach (var frame in attackFrames)
        {
            sr.sprite = frame;
            yield return new WaitForSeconds(interval);
        }
        isAttacking = false;
    }
}
