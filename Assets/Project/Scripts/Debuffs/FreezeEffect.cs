using UnityEngine;

public class FreezeEffect : IOnHitEffect
{
    public OnHitEffectType EffectType => OnHitEffectType.Freeze;

    private float _slowAmount; // от 0 до 1
    private float _duration;   // таймер для стаков (2 секунды)

    public FreezeEffect(float slowAmount, float duration)
    {
        _slowAmount = Mathf.Clamp01(slowAmount);
        _duration = duration;
    }

    public void Apply(Bullet bullet, Enemy target)
    {
        var debuff = new FreezeDebuff(_slowAmount, _duration);
        target.ModifierManager.AddStatusEffect(debuff);
    }
}
