public class LightningEffect : IOnHitEffect
{
    public OnHitEffectType EffectType => OnHitEffectType.Lightning;

    private readonly float _damage;
    private readonly float _radius;
    private readonly int _maxBounces;
    private readonly float _duration;
    private readonly float _ticksPerSecond;

    public LightningEffect(StatusEffectSO so)
    {
        _damage = so.GetParameter(EffectParameterType.Damage);
        _radius = so.GetParameter(EffectParameterType.Radius);
        _maxBounces = (int)so.GetParameter(EffectParameterType.MaxBounces);
        _ticksPerSecond = so.GetParameter(EffectParameterType.TicksPerSecond);
        _duration = so.Duration;
    }

    public void Apply(Bullet bullet, Enemy target)
    {
        LightningVisual lightningVisual = target.GetComponent<LightningVisual>();
        if (lightningVisual == null)
            lightningVisual = target.gameObject.AddComponent<LightningVisual>();

        var debuff = new LightningDebuff(_damage, _radius, _maxBounces, _duration, _ticksPerSecond, lightningVisual);
        target.ModifierManager.AddStatusEffect(debuff);
    }
}
