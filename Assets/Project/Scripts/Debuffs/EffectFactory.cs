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
    public IOnHitEffect CreateEffect(OnHitEffectType type)
    {
        StatusEffectSO so = GetEffectSO(type);
        if (so == null)
        {
            Debug.LogError($"EffectFactory: No StatusEffectSO for {type}");
            return null;
        }

        return type switch
        {
            OnHitEffectType.Fire => new FireEffect(so),
            OnHitEffectType.Ice => new IceEffect(so),
            OnHitEffectType.Freeze => new FreezeEffect(so),
            OnHitEffectType.Lightning => new LightningEffect(so),
            _ => null
        };
    }

    private StatusEffectSO GetEffectSO(OnHitEffectType type)
    {
        foreach (var e in allEffects)
            if (e.Type == type) return e;
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
