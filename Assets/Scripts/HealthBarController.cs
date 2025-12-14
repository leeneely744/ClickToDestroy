using UnityEngine;

public class HealthBarController : MonoBehaviour
{
    private Vector3 initialScale;
    [Range(0f, 1f)]
    [SerializeField] private float ratio = 1f;

    private void Awake()
    {
        initialScale = transform.localScale;
    }

    private void LateUpdate()
    {
        Vector3 scale = initialScale;
        float clamped = Mathf.Clamp01(ratio);
        scale.x *= clamped;
        transform.localScale = scale;
    }

    public void SetRatio(float value)
    {
        ratio = value;
    }
}
