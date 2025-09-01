using UnityEngine;

public class FireEffect : IOnHitEffect
{
    public OnHitEffectType EffectType => OnHitEffectType.Fire;

    private float _duration;
    private float _dps;
    private Sprite _icon;

    public FireEffect(float dps, float duration, Sprite icon)
    {
        _duration = duration;
        _dps = dps;
        _icon = icon;
    }

    public void Apply(Bullet bullet, Enemy target)
    {
        BurnDebuff debuff = new BurnDebuff(_duration, _dps, _icon);
        target.ModifierManager.AddStatusEffect(debuff);
    }
}
