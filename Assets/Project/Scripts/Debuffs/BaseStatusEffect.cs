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

    /// <summary>
    /// Метод обновления эффекта. 
    /// Вызывается только для Refreshable эффектов.
    /// Для Stackable эффекта не используется.
    /// </summary>
    public virtual void Refresh(IStatusEffect newEffect)
    {
        if (StackType == EffectStackType.Refreshable)
        {
            OnRefresh(newEffect);
        }
    }

    /// <summary>
    /// Собственно логика обновления для Refreshable эффектов.
    /// </summary>
    protected abstract void OnRefresh(IStatusEffect newEffect);
}
