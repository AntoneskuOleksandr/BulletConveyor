public abstract class OnHitEffectBase : IOnHitEffect
{
    public abstract OnHitEffectType EffectType { get; }

    protected readonly StatusEffectSO _so;

    protected OnHitEffectBase(StatusEffectSO so)
    {
        _so = so;
    }

    public abstract void Apply(Bullet bullet, Enemy target);
}