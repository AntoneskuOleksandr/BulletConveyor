public class FireEffect : OnHitEffectBase
{
    public override OnHitEffectType EffectType => OnHitEffectType.Fire;

    private float _duration;
    private float _dps;

    public FireEffect(StatusEffectSO so) : base(so)
    {
        _duration = so.Duration;
        _dps = so.GetParameter(EffectParameterType.DPS);
    }

    public override void Apply(ProjectileBase projectile, Enemy target)
    {
        var debuff = new BurnDebuff(_dps, _duration);
        target.ModifierManager.AddStatusEffect(debuff);
    }
}
