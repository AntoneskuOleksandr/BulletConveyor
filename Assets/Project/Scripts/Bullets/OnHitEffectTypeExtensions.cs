using UnityEngine;

public static class OnHitEffectTypeExtensions
{
    public static IOnHitEffect CreateEffect(this OnHitEffectType type)
    {
        switch (type)
        {
            case OnHitEffectType.Fire: return new FireEffect(3f, 5f);
            case OnHitEffectType.Freeze: return new FreezeEffect(0.3f, 2f, 0.1f);
            // case OnHitEffectType.Ice: return new IceEffect(...);
            default:
                Debug.LogWarning($"Effect {type} not implemented!");
                return null;
        }
    }
}
