using UnityEngine;

public class BurnDebuff : BaseStatusEffect, ITickableEffect
{
    public override OnHitEffectType EffectType => OnHitEffectType.Fire;
    public override EffectStackType StackType => EffectStackType.Refreshable;

    public float DPS { get; private set; }

    public BurnDebuff(float dps, float duration)
    {
        DPS = Mathf.Max(0f, dps);
        Duration = duration;
        Elapsed = 0f;
    }

    public void Tick(float deltaTime, Enemy enemy)
    {
        if (IsFinished) return;

        Elapsed += deltaTime;
        enemy.TakeDamage(DPS * deltaTime);
    }

    protected override void OnRefresh(IStatusEffect newEffect)
    {
        if (newEffect is BurnDebuff burn)
        {
            Duration = Mathf.Max(Duration, burn.Duration);
            DPS += burn.DPS;
            Elapsed = 0f;
        }
    }
}
