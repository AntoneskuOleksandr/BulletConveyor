public class IceEffect : IOnHitEffect
{
    public OnHitEffectType EffectType => OnHitEffectType.Ice;

    private float _duration;

    public IceEffect(float duration)
    {
        _duration = duration;
    }

    public void Apply(Bullet bullet, Enemy target)
    {
        var debuff = new IceDebuff(_duration);
        target.ModifierManager.AddStatusEffect(debuff);
    }
}
