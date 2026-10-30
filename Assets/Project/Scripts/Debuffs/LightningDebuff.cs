using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class LightningDebuff : BaseStatusEffect, ITickableEffect, IModifierEffect
{
    public override OnHitEffectType EffectType => OnHitEffectType.Lightning;
    public override EffectStackType StackType => EffectStackType.Refreshable;

    private float _slow;

    public LightningDebuff(float slowAmount, float duration)
    {
        _slow = Mathf.Clamp01(slowAmount);
        Duration = Mathf.Max(0.01f, duration);
        Elapsed = 0f;
    }

    public void Tick(float deltaTime, Enemy enemy)
    {
        if (IsFinished) return;

        Elapsed += deltaTime;
    }

    public Dictionary<ModifierType, float> GetModifiers()
    {
        return new Dictionary<ModifierType, float>
        {
            { ModifierType.Slow, _slow }
        };
    }

    protected override void OnRefresh(IStatusEffect newEffect)
    {
        if (newEffect is LightningDebuff lightning)
        {
            Duration = Mathf.Max(Duration, lightning.Duration);
            Elapsed = 0f;
        }
    }
}
