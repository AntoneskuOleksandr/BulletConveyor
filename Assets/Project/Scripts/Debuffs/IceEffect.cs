public class IceEffect : IOnHitEffect
{
    public OnHitEffectType EffectType => OnHitEffectType.Ice;

    private readonly float _meltSpeed;
    private readonly float _duration;

    public IceEffect(StatusEffectSO so)
    {
        _meltSpeed = so.GetParameter(EffectParameterType.MeltSpeed);
        _duration = so.Duration;
    }

    public void Apply(ProjectileBase projectile, Enemy target)
    {
        var debuff = new IceDebuff(_meltSpeed, _duration);
        target.ModifierManager.AddStatusEffect(debuff);
    }
}
