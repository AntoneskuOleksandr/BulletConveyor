public interface IStatusEffect
{
    OnHitEffectType EffectType { get; }

    // Старт работы дебаффа (обычно запускает корутину на враге)
    void Start(Enemy enemy);

    // Обновление при повторном наложении (продлевает таймер, усиливает эффект и т.п.)
    void Refresh(IStatusEffect newEffect);

    // Принудительное снятие (если враг умер или истёк таймер)
    void Stop();
}
