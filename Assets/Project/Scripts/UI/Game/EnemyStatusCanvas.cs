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

    // Для Refreshable: тип → один UI
    private Dictionary<OnHitEffectType, StatusEffectUI> _refreshableEffects = new();

    // Для Stackable: тип → список UI
    private Dictionary<OnHitEffectType, List<StatusEffectUI>> _stackableEffects = new();

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

        if (effect.StackType == EffectStackType.Refreshable)
        {
            if (_refreshableEffects.ContainsKey(effect.EffectType))
            {
                _refreshableEffects[effect.EffectType].Initialize(icon, effect);
                return;
            }

            CreateUI(effect, icon, _refreshableEffects);
        }
        else // Stackable
        {
            if (!_stackableEffects.TryGetValue(effect.EffectType, out var list))
            {
                list = new List<StatusEffectUI>();
                _stackableEffects[effect.EffectType] = list;
            }

            CreateUI(effect, icon, list);
        }
    }

    public void RemoveStatusEffect(IStatusEffect effect)
    {
        if (effect.StackType == EffectStackType.Refreshable)
        {
            if (_refreshableEffects.TryGetValue(effect.EffectType, out var ui))
            {
                Destroy(ui.gameObject);
                _refreshableEffects.Remove(effect.EffectType);
            }
        }
        else // Stackable
        {
            if (_stackableEffects.TryGetValue(effect.EffectType, out var list))
            {
                StatusEffectUI toRemove = list.Find(ui => ui.Effect == effect);
                if (toRemove != null)
                {
                    Destroy(toRemove.gameObject);
                    list.Remove(toRemove);
                }

                if (list.Count == 0)
                    _stackableEffects.Remove(effect.EffectType);
            }
        }
    }

    private void CreateUI(IStatusEffect effect, Sprite icon, Dictionary<OnHitEffectType, StatusEffectUI> dict)
    {
        if (effectUIPrefab == null || effectsContainer == null) return;

        GameObject uiObj = Instantiate(effectUIPrefab, effectsContainer);
        StatusEffectUI ui = uiObj.GetComponent<StatusEffectUI>();
        ui.Initialize(icon, effect);

        dict[effect.EffectType] = ui;
    }

    private void CreateUI(IStatusEffect effect, Sprite icon, List<StatusEffectUI> list)
    {
        if (effectUIPrefab == null || effectsContainer == null) return;

        GameObject uiObj = Instantiate(effectUIPrefab, effectsContainer);
        StatusEffectUI ui = uiObj.GetComponent<StatusEffectUI>();
        ui.Initialize(icon, effect);

        list.Add(ui);
    }
    #endregion
}
