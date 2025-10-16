using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ƒебаф: замедление врага (Freeze)
/// </summary>
public class FreezeDebuff : BaseStatusEffect, ITickableEffect, IModifierEffect
{
    public override OnHitEffectType EffectType => OnHitEffectType.Freeze;
    public override EffectStackType StackType => EffectStackType.Refreshable;

    private float _slow;         // текущий процент замедлени€ (0..1)
    private float _stackedSlow;  // прирост замедлени€ при повторном попадании
    private float _meltSpeed;    // ускорение та€ни€ от огн€

    private const float BASE_MELT_SPEED = 0.3f;

    public float Strength => _slow;

    public override bool IsFinished => _slow <= 0f;

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
        if (_slow <= 0f) return;

        if (Elapsed < Duration)
        {
            Elapsed += deltaTime * _meltSpeed;
        }
        else
        {
            float meltFactor = _meltSpeed * BASE_MELT_SPEED;
            _slow -= meltFactor * deltaTime;
            _slow = Mathf.Max(0f, _slow);
        }

        // ѕримен€ем замедление напр€мую
        enemy.SetSpeed(enemy.BaseSpeed * (1f - _slow));
    }

    protected override void OnRefresh(IStatusEffect newEffect)
    {
        if (newEffect is FreezeDebuff freeze)
        {
            _slow = Mathf.Clamp01(_slow + freeze._stackedSlow);
            _stackedSlow = freeze._stackedSlow;

            Duration = Mathf.Max(Duration, freeze.Duration);
            Elapsed = 0f;
        }
    }

    public Dictionary<ModifierType, float> GetModifiers()
    {
        return new Dictionary<ModifierType, float> { { ModifierType.Slow, _slow } };
    }

    public void StartMelt(float fireMeltSpeed)
    {
        _meltSpeed = Mathf.Max(_meltSpeed, fireMeltSpeed);
    }
}