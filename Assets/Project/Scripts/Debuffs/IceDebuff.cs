using System.Collections.Generic;
using UnityEngine;

public class IceDebuff : BaseStatusEffect, ITickableEffect, IModifierEffect
{
    public override OnHitEffectType EffectType => OnHitEffectType.Ice;
    public override EffectStackType StackType => EffectStackType.Refreshable;

    private float _meltSpeed;

    public IceDebuff(float meltSpeed, float duration)
    {
        _meltSpeed = meltSpeed;
        Duration = duration;
        Elapsed = 0f;
    }

    public void Tick(float deltaTime, Enemy enemy)
    {
        if (IsFinished) return;

        Elapsed += deltaTime * _meltSpeed;
    }

    protected override void OnRefresh(IStatusEffect newEffect)
    {
        if (newEffect is IceDebuff ice)
        {
            Duration = Mathf.Max(Duration, ice.Duration);
            Elapsed = 0f;
        }
    }

    public Dictionary<ModifierType, float> GetModifiers()
    {
        return new Dictionary<ModifierType, float>
        {
            { ModifierType.Slow, 1f }
        };
    }

    public void StartMelt(float fireMeltSpeed)
    {
        _meltSpeed = Mathf.Max(_meltSpeed, fireMeltSpeed);
    }
}
