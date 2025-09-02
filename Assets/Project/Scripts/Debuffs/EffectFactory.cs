using UnityEngine;

public class EffectFactory : MonoBehaviour
{
    [SerializeField] private Sprite fireIcon;
    [SerializeField] private Sprite freezeIcon;
    [SerializeField] private Sprite iceIcon;

    [SerializeField] private StatusEffectSO[] allEffects; // массив всех ScriptableObject эффектов

    public IOnHitEffect CreateEffect(OnHitEffectType type)
    {
        StatusEffectSO data = GetEffectSO(type);
        if (data == null)
        {
            Debug.LogError($"There is no {type} effect assigned!");
            return null;
        }

        switch (type)
        {
            case OnHitEffectType.Fire:
                return new FireEffect(data.GetParameter("DPS"), data.Duration);
            case OnHitEffectType.Freeze:
                return new FreezeEffect(data.GetParameter("Strength"), data.Duration);
            case OnHitEffectType.Ice:
                return new IceEffect(data.GetParameter("MeltSpeed"), data.Duration);
            default:
                return null;
        }
    }

    private StatusEffectSO GetEffectSO(OnHitEffectType type)
    {
        foreach (var e in allEffects)
            if (e.Type == type) return e;
        return null;
    }

    public Sprite GetIconByEffectType(OnHitEffectType type)
    {
        return type switch
        {
            OnHitEffectType.Fire => fireIcon,
            OnHitEffectType.Freeze => freezeIcon,
            OnHitEffectType.Ice => iceIcon,
            _ => null
        };
    }
}
