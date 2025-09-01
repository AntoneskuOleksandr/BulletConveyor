using UnityEngine;

public class FireEffect : IOnHitEffect
{
    public OnHitEffectType EffectType => OnHitEffectType.Fire;

    private float _duration;
    private float _dps;
    private Sprite _icon;

    public FireEffect(float duration, float dps, Sprite icon)
    {
        _duration = duration;
        _dps = dps;
        _icon = icon;
    }

    public void Apply(Bullet bullet, Enemy target)
    {
        // Создаём дебафф и передаём его врагу
        var debuff = new BurnDebuff(_duration, _dps, _icon);
        target.AddStatusEffect(debuff);
    }
}
