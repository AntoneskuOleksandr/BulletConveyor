using UnityEngine;

/// <summary>
/// Абстрактный базовый класс для всех эффектов
/// </summary>
public abstract class BaseStatusEffect : IStatusEffect
{
    public abstract OnHitEffectType EffectType { get; }
    public abstract EffectStackType StackType { get; }

    public float Duration { get; protected set; }
    public float Elapsed { get; protected set; }
    public float DurationLeft => Mathf.Max(Duration - Elapsed, 0f);
    public virtual bool IsFinished => Elapsed >= Duration;

    public abstract void Refresh(IStatusEffect newEffect);
}
