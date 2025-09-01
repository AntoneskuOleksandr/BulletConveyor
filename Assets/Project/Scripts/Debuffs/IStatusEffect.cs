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
    /// Иконка эффекта для UI.
    /// </summary>
    Sprite Icon { get; }

    /// <summary>
    /// Старт работы дебаффа (обычно запускает корутину на враге).
    /// </summary>
    void Start(Enemy enemy);

    /// <summary>
    /// Обновление при повторном наложении (продлевает таймер, усиливает эффект).
    /// </summary>
    void Refresh(IStatusEffect newEffect);

    /// <summary>
    /// Принудительное снятие эффекта.
    /// </summary>
    void Stop();
}
