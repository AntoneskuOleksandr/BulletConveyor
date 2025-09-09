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

    /// <summary>Tick вызывается каждый кадр</summary>
    public void Tick(float deltaTime, Enemy enemy)
    {
        if (IsFinished) return;

        Elapsed += deltaTime * _meltSpeed;
    }

    /// <summary>Обновление при повторном наложении</summary>
    public override void Refresh(IStatusEffect newEffect)
    {
        if (newEffect is IceDebuff ice)
        {
            Duration = Mathf.Max(Duration, ice.Duration);
            Elapsed = 0f;
        }
    }

    /// <summary>Модификаторы для влияния на врага</summary>
    public Dictionary<ModifierType, float> GetModifiers()
    {
        return new Dictionary<ModifierType, float>
        {
            { ModifierType.Slow, 1f } // полностью замораживает
        };
    }

    public void StartMelt(float fireMeltSpeed)
    {
        _meltSpeed = Mathf.Max(_meltSpeed, fireMeltSpeed);
    }
}