using UnityEngine;

public interface IStatusEffect
{
    OnHitEffectType EffectType { get; }

    /// <summary>
    /// Длительность эффекта (в секундах).
    /// </summary>
    float Duration { get; }

    /// <summary>
    /// Оставшееся время действия эффекта (в секундах).
    /// </summary>
    float DurationLeft { get; }

    /// <summary>
    /// Обновление при повторном наложении (продлевает таймер, усиливает эффект).
    /// </summary>
    void Refresh(IStatusEffect newEffect);
}
