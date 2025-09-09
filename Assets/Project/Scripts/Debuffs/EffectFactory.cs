using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Фабрика для создания эффектов (IOnHitEffect)
/// </summary>
public class EffectFactory : MonoBehaviour
{
    [Header("ScriptableObjects Effects")]
    [SerializeField] private StatusEffectSO[] allEffects;

    [Header("Effect Icons")]
    [SerializeField] private Sprite fireIcon;
    [SerializeField] private Sprite freezeIcon;
    [SerializeField] private Sprite iceIcon;
    [SerializeField] private Sprite lightningIcon;

    // Словарь для регистрации кастомных сложных эффектов
    private readonly Dictionary<OnHitEffectType, Func<StatusEffectSO, IOnHitEffect>> _specialEffectCreators
        = new Dictionary<OnHitEffectType, Func<StatusEffectSO, IOnHitEffect>>();

    private void Awake()
    {
        // Регистрируем стандартные эффекты
        RegisterSpecialEffect(OnHitEffectType.Fire, so => new FireEffect(so.GetParameter("DPS"), so.Duration));
        RegisterSpecialEffect(OnHitEffectType.Ice, so => new IceEffect(so.GetParameter("MeltSpeed"), so.Duration));
        RegisterSpecialEffect(OnHitEffectType.Freeze, so => new FreezeEffect(so.GetParameter("Strength"), so.Duration));
        RegisterSpecialEffect(OnHitEffectType.Lightning, so => new LightningEffect(
            so.GetParameter("DamagePerTick"),
            so.GetParameter("Radius"),
            (int)so.GetParameter("MaxBounces"),
            so.GetParameter("TicksPerSecond"),
            so.Duration));
    }

    /// <summary>
    /// Регистрирует кастомный/сложный эффект
    /// </summary>
    public void RegisterSpecialEffect(OnHitEffectType type, Func<StatusEffectSO, IOnHitEffect> creator)
    {
        _specialEffectCreators[type] = creator;
    }

    /// <summary>
    /// Создаёт эффект по типу
    /// </summary>
    public IOnHitEffect CreateEffect(OnHitEffectType type)
    {
        StatusEffectSO so = GetEffectSO(type);
        if (so == null)
        {
            Debug.LogError($"EffectFactory: No StatusEffectSO assigned for {type}");
            return null;
        }

        if (_specialEffectCreators.TryGetValue(type, out var creator))
            return creator.Invoke(so);

        Debug.LogError($"EffectFactory: No creator registered for {type}");
        return null;
    }

    /// <summary>
    /// Получение ScriptableObject для типа эффекта
    /// </summary>
    private StatusEffectSO GetEffectSO(OnHitEffectType type)
    {
        foreach (var e in allEffects)
        {
            if (e.Type == type) return e;
        }
        return null;
    }

    /// <summary>
    /// Получение иконки для UI
    /// </summary>
    public Sprite GetIconByEffectType(OnHitEffectType type)
    {
        return type switch
        {
            OnHitEffectType.Fire => fireIcon,
            OnHitEffectType.Freeze => freezeIcon,
            OnHitEffectType.Ice => iceIcon,
            OnHitEffectType.Lightning => lightningIcon,
            _ => null
        };
    }
}
