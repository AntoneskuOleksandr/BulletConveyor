using UnityEngine;

public class BurnDebuff : ITickEffect
{
    public OnHitEffectType EffectType => OnHitEffectType.Fire;
    public float Duration { get; private set; }
    public float Elapsed { get; private set; }
    public float DPS { get; private set; }
    public float DurationLeft => Mathf.Max(Duration - Elapsed, 0f);

    public BurnDebuff(float dps, float duration)
    {
        DPS = Mathf.Max(0f, dps);
        Duration = duration;
        Elapsed = 0f;
    }

    public void Tick(float deltaTime, Enemy enemy)
    {
        if (Elapsed < Duration)
        {
            Elapsed += deltaTime;
            // Наносим урон от горения
            enemy.TakeDamage(DPS * deltaTime);
        }
    }

    public void Refresh(IStatusEffect newEffect)
    {
        if (newEffect is BurnDebuff burn)
        {
            // обновляем время и увеличиваем dps
            Duration = Mathf.Max(Duration, burn.Duration);
            DPS += burn.DPS;
            Elapsed = 0f;
        }
    }

    public bool IsFinished() => Elapsed >= Duration;
}
