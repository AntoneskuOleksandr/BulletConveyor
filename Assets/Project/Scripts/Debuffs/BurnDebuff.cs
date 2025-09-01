using UnityEngine;

public class BurnDebuff : ITickEffect
{
    public OnHitEffectType EffectType => OnHitEffectType.Fire;
    public float Duration { get; private set; }
    public float Elapsed { get; private set; }
    public float DPS { get; private set; }
    public Sprite Icon { get; private set; }
    public float DurationLeft => Mathf.Max(Duration - Elapsed, 0f);

    public BurnDebuff(float duration, float dps, Sprite icon)
    {
        Duration = duration;
        DPS = dps;
        Icon = icon;
        Elapsed = 0f;
    }

    public void Tick(float deltaTime, Enemy enemy)
    {
        if (Elapsed < Duration && enemy.IsAlive)
        {
            enemy.TakeDamage(DPS * deltaTime);
            Elapsed += deltaTime;
        }
    }

    public void Refresh(IStatusEffect newEffect)
    {
        if (newEffect is BurnDebuff burn)
        {
            Duration = Mathf.Max(Duration, burn.Duration);
            DPS = Mathf.Max(DPS, burn.DPS);
            Elapsed = 0f;
        }
    }

    public bool IsFinished() => Elapsed >= Duration;
}
