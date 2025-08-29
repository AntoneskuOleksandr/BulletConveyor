using UnityEngine;
using static EffectChanceManager;

public static class BulletExtensions
{
    public static IProjectileEffect AddComponentByEffectType(this GameObject go, ProjectileEffectType type)
    {
        switch (type)
        {
            case ProjectileEffectType.Fire: return go.AddComponent<FireEffect>();
            default:
                Debug.LogWarning($"Effect {type} not implemented!");
                return null;
        }
    }
}
