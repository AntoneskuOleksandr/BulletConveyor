using UnityEngine;

public class FreezeEffect : IOnHitEffect
{
    public OnHitEffectType EffectType => OnHitEffectType.Freeze;

    private float _slowAmount; // от 0 до 1
    private float _duration;   // таймер для стаков (2 секунды)
    private Sprite _icon;

    public FreezeEffect(float slowAmount, float duration, Sprite icon)
    {
        _slowAmount = Mathf.Clamp01(slowAmount);
        _duration = duration;
        _icon = icon;
    }

    public void Apply(Bullet bullet, Enemy target)
    {
        var debuff = new FreezeDebuff(_slowAmount, _duration, _icon);
        target.ModifierManager.AddStatusEffect(debuff);
    }
}
