using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class EnemyStatusCanvas : MonoBehaviour
{
    [Header("Health Bar")]
    [SerializeField] private GameObject healthBar;

    [Header("Status Effects")]
    [SerializeField] private Transform effectsContainer;
    [SerializeField] private GameObject effectUIPrefab;

    [Inject] private EffectFactory _effectFactory;

    private Dictionary<OnHitEffectType, StatusEffectUI> _activeEffects = new();

    #region Health Bar
    public void SetHealthBarFillAmount(float fillAmount)
    {
        if (!this.gameObject.activeSelf)
            this.gameObject.SetActive(true);

        if (healthBar != null)
        {
            var fillImage = healthBar.GetComponent<Image>();
            if (fillImage != null)
                fillImage.fillAmount = Mathf.Clamp01(fillAmount);
        }
    }

    public void HideHealthBar() => this.gameObject.SetActive(false);
    #endregion

    #region Status Effects
    public void AddStatusEffect(IStatusEffect effect)
    {
        Sprite icon = _effectFactory.GetIconByEffectType(effect.EffectType);

        if (_activeEffects.ContainsKey(effect.EffectType))
        {
            // Обновляем UI существующего эффекта
            _activeEffects[effect.EffectType].Initialize(icon, effect);
            return;
        }

        if (effectUIPrefab == null || effectsContainer == null) return;

        GameObject uiObj = Instantiate(effectUIPrefab, effectsContainer);
        StatusEffectUI ui = uiObj.GetComponent<StatusEffectUI>();
        ui.Initialize(icon, effect);

        _activeEffects[effect.EffectType] = ui;
    }

    public void RemoveStatusEffect(OnHitEffectType type)
    {
        if (_activeEffects.TryGetValue(type, out StatusEffectUI ui))
        {
            Destroy(ui.gameObject);
            _activeEffects.Remove(type);
        }
    }
    #endregion
}
