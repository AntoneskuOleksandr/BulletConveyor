public interface IOnHitEffect
{
    OnHitEffectType EffectType { get; }

    void Apply(ProjectileBase projectile, Enemy target);
}

public enum OnHitEffectType
{
    Fire,
    Freeze,
    Ice,
    Lightning
}
