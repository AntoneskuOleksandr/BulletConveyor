using System.Collections.Generic;
using UnityEngine;

public class FreezeDebuff : ITickEffect, IModifierEffect
{
    public OnHitEffectType EffectType => OnHitEffectType.Freeze;
    public EffectStackType StackType => EffectStackType.Refreshable;
    public float Duration { get; private set; }   // активная фаза заморозки
    public float Elapsed { get; private set; }
    public float DPS => 0f;
    public float DurationLeft => Mathf.Max(Duration - Elapsed, 0f);
    public float Strength => _slow; // сила замедления (0..1)

    private float _slow;           // текущий процент замедления
    private float _stackedSlow;    // прирост замедления при попадании
    private float _meltSpeed;      // ускорение таяния от огня

    private const float BASE_MELT_SPEED = 0.3f;

    public FreezeDebuff(float slowAmount, float duration)
    {
        Duration = duration;
        Elapsed = 0f;
        _slow = Mathf.Clamp01(slowAmount);
        _stackedSlow = _slow;
        _meltSpeed = 1f;
    }

    public void Tick(float deltaTime, Enemy enemy)
    {
        if (_slow <= 0f)
            return;

        if (Elapsed < Duration)
        {
            Elapsed += deltaTime * _meltSpeed;
        }
        else
        {
            // Фаза таяния после окончания Duration
            float meltFactor = _meltSpeed * BASE_MELT_SPEED;
            _slow -= meltFactor * deltaTime;
            _slow = Mathf.Max(0f, _slow);
        }

        // Применяем замедление напрямую
        enemy.SetSpeed(enemy.BaseSpeed * (1f - _slow));
    }

    public void Refresh(IStatusEffect newEffect)
    {
        if (newEffect is FreezeDebuff freeze)
        {
            // стакуем замедление
            _slow = Mathf.Clamp01(_slow + freeze._stackedSlow);
            _stackedSlow = freeze._stackedSlow;

            // продлеваем активную фазу
            Duration = Mathf.Max(Duration, freeze.Duration);
            Elapsed = 0f;
        }
    }

    public bool IsFinished() => _slow <= 0f;

    public void StartMelt(float fireMeltSpeed)
    {
        _meltSpeed = Mathf.Max(_meltSpeed, fireMeltSpeed);
    }

    public Dictionary<ModifierType, float> GetModifiers()
    {
        return new Dictionary<ModifierType, float> { { ModifierType.Slow, _slow } };
    }
}
