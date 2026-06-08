using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class HeroButton : MonoBehaviour
{
    [SerializeField] private HeroController heroController;
    [SerializeField] private GameObject heroButtonLight;

    private Button button;

    void Awake()
    {
        if (heroController == null)
            heroController = FindAnyObjectByType<HeroController>();
        button = GetComponent<Button>();

        if (heroController == null)
        {
            Debug.LogError("[HeroButton] HeroController が見つかりません。", this);
            return;
        }

        if (heroButtonLight != null)
            heroButtonLight.SetActive(false);

        heroController.OnSelectStateChanged += OnHeroSelectChanged;
        heroController.OnDeadStateChanged += OnHeroDeadChanged;
    }

    void OnDestroy()
    {
        heroController.OnSelectStateChanged -= OnHeroSelectChanged;
        heroController.OnDeadStateChanged -= OnHeroDeadChanged;
    }

    private void OnHeroSelectChanged(bool isSelected)
    {
        if (heroButtonLight != null)
            heroButtonLight.SetActive(isSelected);
    }

    private void OnHeroDeadChanged(bool isDead)
    {
        button.interactable = !isDead;
    }
}
