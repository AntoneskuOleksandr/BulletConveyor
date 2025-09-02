using UnityEngine;
using UnityEngine.UI;

public class StatusEffectUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;  // Filled Image с Radial360
    private IStatusEffect effect;

    public void Initialize(Sprite icon, IStatusEffect effect)
    {
        iconImage.sprite = icon;
        this.effect = effect;
        iconImage.fillAmount = 1f;
        Debug.Log($"Effect: {effect.EffectType}; Duration: {effect.Duration}");
    }

    private void Update()
    {
        // Используем оставшееся время
        float fill = Mathf.Clamp01(effect.DurationLeft / effect.Duration);
        iconImage.fillAmount = fill;
    }
}
