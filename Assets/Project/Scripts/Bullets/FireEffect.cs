using UnityEngine;

public class FireEffect : IOnHitEffect
{
    public OnHitEffectType EffectType => OnHitEffectType.Fire;

    private float _duration;
    private float _dps;

    public FireEffect(float duration, float dps)
    {
        _duration = duration;
        _dps = dps;
    }

    public void Apply(Bullet bullet, Enemy target)
    {
        // Создаём дебафф и передаём его врагу
        var debuff = new BurnDebuff(_duration, _dps);
        target.AddStatusEffect(debuff);
    }
}
