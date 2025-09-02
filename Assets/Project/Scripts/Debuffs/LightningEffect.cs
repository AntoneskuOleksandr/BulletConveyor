using UnityEngine;

public class LightningEffect : IOnHitEffect
{
    public OnHitEffectType EffectType => OnHitEffectType.Lightning;

    private float _damage;
    private float _radius;
    private int _maxBounces;
    private float _duration;
    private float _ticksPerSecond;

    public LightningEffect(float damage, float radius, int maxBounces, float ticksPerSecond, float duration)
    {
        _damage = damage;
        _radius = radius;
        _maxBounces = maxBounces;
        _ticksPerSecond = ticksPerSecond;
        _duration = duration;
    }

    public void Apply(Bullet bullet, Enemy target)
    {
        var debuff = new LightningDebuff(_damage, _radius, _maxBounces, _duration, _ticksPerSecond);
        target.ModifierManager.AddStatusEffect(debuff);
    }
}
