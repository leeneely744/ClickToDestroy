using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class HeroButton : MonoBehaviour
{
    [SerializeField] private HeroController heroController;
    [SerializeField] private Image buttonImage;
    [SerializeField] private Color glowColor = new Color(1f, 0.9f, 0.3f);
    [SerializeField] private float pulseSpeed = 3f;

    private Color normalColor;
    private Coroutine pulseCoroutine;
    private Button button;

    void Awake()
    {
        if (heroController == null)
            heroController = FindAnyObjectByType<HeroController>();
        if (buttonImage == null)
            buttonImage = GetComponent<Image>();
        button = GetComponent<Button>();

        if (heroController == null)
        {
            Debug.LogError("[HeroButton] HeroController が見つかりません。", this);
            return;
        }

        normalColor = buttonImage.color;
        heroController.OnSelectStateChanged += OnHeroSelectChanged;
        heroController.OnDeadStateChanged += OnHeroDeadChanged;
    }

    void OnDestroy()
    {
        heroController.OnSelectStateChanged -= OnHeroSelectChanged;
        heroController.OnDeadStateChanged -= OnHeroDeadChanged;
    }

    private void OnHeroDeadChanged(bool isDead)
    {
        button.interactable = !isDead;
    }

    private void OnHeroSelectChanged(bool isSelected)
    {
        if (pulseCoroutine != null)
            StopCoroutine(pulseCoroutine);

        if (isSelected)
            pulseCoroutine = StartCoroutine(PulseEffect());
        else
            buttonImage.color = normalColor;
    }

    private IEnumerator PulseEffect()
    {
        while (true)
        {
            buttonImage.color = Color.Lerp(normalColor, glowColor, Mathf.PingPong(Time.time * pulseSpeed, 1f));
            yield return null;
        }
    }
}
