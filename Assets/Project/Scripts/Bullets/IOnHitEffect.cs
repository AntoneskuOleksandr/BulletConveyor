public interface IOnHitEffect
{
    OnHitEffectType EffectType { get; }

    // Вызывается пулей при попадании во врага
    void Apply(Bullet bullet, Enemy target);
}

public enum OnHitEffectType
{
    Fire,
    Ice,
    Poison,
    Explosion
}
