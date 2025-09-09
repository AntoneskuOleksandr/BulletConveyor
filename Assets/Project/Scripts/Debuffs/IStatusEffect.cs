using System;

/// <summary>
/// Основной интерфейс любого статус-эффекта (баф/дебаф/мгновенный эффект)
/// </summary>
public interface IStatusEffect
{
    OnHitEffectType EffectType { get; }
    EffectStackType StackType { get; }

    /// <summary>Общая длительность эффекта в секундах</summary>
    float Duration { get; }

    /// <summary>Прошедшее время с момента наложения эффекта</summary>
    float Elapsed { get; }

    /// <summary>Оставшееся время действия эффекта</summary>
    float DurationLeft { get; }

    /// <summary>Флаг завершения эффекта</summary>
    bool IsFinished { get; }

    /// <summary>Обновление при повторном наложении</summary>
    void Refresh(IStatusEffect newEffect);
}

public enum EffectStackType
{
    Refreshable,   // перезаписываемый: новый эффект обновляет старый
    Stackable      // накапливаемый: каждый экземпляр отдельный
}
