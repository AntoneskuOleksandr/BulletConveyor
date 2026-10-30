using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class EffectFactory : MonoBehaviour
{
    [Header("ScriptableObjects Effects")]
    [SerializeField] private StatusEffectSO[] allEffects;

    [Header("Effect Icons")]
    [SerializeField] private Sprite fireIcon;
    [SerializeField] private Sprite freezeIcon;
    [SerializeField] private Sprite iceIcon;
    [SerializeField] private Sprite lightningIcon;

    [Inject] private LightningManager _lightningManager;

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
            OnHitEffectType.Lightning => new LightningEffect(so, _lightningManager),
            _ => null
        };
    }

    private StatusEffectSO GetEffectSO(OnHitEffectType type)
    {
        foreach (var e in allEffects)
            if (e.Type == type) return e;
        return null;
    }

    public Dictionary<Color, float> ConvertChancesToColors(Dictionary<OnHitEffectType, float> chances)
    {
        var result = new Dictionary<Color, float>();

        foreach (var kv in chances)
        {
            StatusEffectSO so = GetEffectSO(kv.Key);
            if (so == null)
                continue;

            if (result.ContainsKey(so.Color))
                result[so.Color] += kv.Value;
            else
                result[so.Color] = kv.Value;
        }

        return result;
    }

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
