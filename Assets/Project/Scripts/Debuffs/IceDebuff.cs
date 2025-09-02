using System.Collections.Generic;
using UnityEngine;

public class IceDebuff : ITickEffect, IModifierEffect
{
    public OnHitEffectType EffectType => OnHitEffectType.Ice;
    public float Duration { get; private set; }
    public float Elapsed { get; private set; }
    public float DPS => 0f;
    public float DurationLeft => Mathf.Max(Duration - Elapsed, 0f);

    private float _meltSpeed; // скорость таяния от внешних факторов

    public IceDebuff(float meltSpeed, float duration)
    {
        _meltSpeed = meltSpeed;
        Duration = duration;
        Elapsed = 0f;
    }

    public void Tick(float deltaTime, Enemy enemy)
    {
        if (Elapsed < Duration)
        {
            Elapsed += deltaTime * _meltSpeed;
        }

        // Полностью блокируем движение
        enemy.SetSpeed(0f);
    }

    public void Refresh(IStatusEffect newEffect)
    {
        // Полностью замороженный враг не стакует лед,
        // но можно обновлять время действия, если нужно
        if (newEffect is IceDebuff ice)
        {
            Duration = Mathf.Max(Duration, ice.Duration);
            Elapsed = 0f;
        }
    }

    public bool IsFinished() => Elapsed >= Duration;

    public void StartMelt(float fireMeltSpeed)
    {
        _meltSpeed = Mathf.Max(_meltSpeed, fireMeltSpeed);
    }

    public Dictionary<ModifierType, float> GetModifiers()
    {
        return new Dictionary<ModifierType, float> { { ModifierType.Slow, 1f } };
    }
}
