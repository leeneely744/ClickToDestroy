using Coffee.UIEffects;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(UIEffect))]
public class HeroButton : MonoBehaviour
{
    [SerializeField] private HeroController heroController;

    private Button button;
    private UIEffect uiEffect;

    void Awake()
    {
        if (heroController == null)
            heroController = FindAnyObjectByType<HeroController>();
        button = GetComponent<Button>();
        uiEffect = GetComponent<UIEffect>();
        uiEffect.enabled = false;

        if (heroController == null)
        {
            Debug.LogError("[HeroButton] HeroController が見つかりません。", this);
            return;
        }

        heroController.OnDeadStateChanged += OnHeroDeadChanged;
        button.onClick.AddListener(OnClick);
    }

    void OnDestroy()
    {
        heroController.OnDeadStateChanged -= OnHeroDeadChanged;
        button.onClick.RemoveListener(OnClick);
    }

    private void OnClick()
    {
        uiEffect.enabled = true;
    }

    public void DeactivateEffect()
    {
        uiEffect.enabled = false;
    }

    private void OnHeroDeadChanged(bool isDead)
    {
        button.interactable = !isDead;
    }
}
