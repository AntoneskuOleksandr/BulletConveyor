using UnityEngine;

public class FreezeEffect : IOnHitEffect
{
    public OnHitEffectType EffectType => OnHitEffectType.Freeze;

    private readonly float _slowAmount;
    private readonly float _duration;

    public FreezeEffect(StatusEffectSO so)
    {
        _slowAmount = Mathf.Clamp01(so.GetParameter(EffectParameterType.SlowAmount));
        _duration = so.Duration;
    }

    public void Apply(ProjectileBase projectileBase, Enemy target)
    {
        var debuff = new FreezeDebuff(_slowAmount, _duration);
        target.ModifierManager.AddStatusEffect(debuff);
    }
}
