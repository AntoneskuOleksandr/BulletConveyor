public interface IStatusEffect
{
    OnHitEffectType EffectType { get; }

    /// <summary>
    /// Оставшееся время действия эффекта (в секундах).
    /// </summary>
    float DurationLeft { get; }

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
