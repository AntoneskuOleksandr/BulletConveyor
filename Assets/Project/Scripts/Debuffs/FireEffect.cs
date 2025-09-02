using UnityEngine;

public class FireEffect : IOnHitEffect
{
    public OnHitEffectType EffectType => OnHitEffectType.Fire;

    private float _duration;
    private float _dps;

    public FireEffect(float dps, float duration)
    {
        _duration = duration;
        _dps = dps;
    }

    public void Apply(Bullet bullet, Enemy target)
    {
        BurnDebuff debuff = new BurnDebuff(_dps, _duration);
        target.ModifierManager.AddStatusEffect(debuff);
    }
}
