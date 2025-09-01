using System.Collections;
using UnityEngine;

public class BurnDebuff : ITickEffect
{
    public OnHitEffectType EffectType => OnHitEffectType.Fire;

    public float Duration { get; private set; }
    public float Elapsed { get; private set; }
    public float DPS { get; private set; }
    public float DurationLeft => Mathf.Max(Duration - Elapsed, 0f);

    public Sprite Icon { get; private set; }

    private Coroutine _burnRoutine;
    private Enemy _enemy;

    public BurnDebuff(float duration, float dps, Sprite icon)
    {
        Duration = duration;
        DPS = dps;
        Elapsed = 0f;

        Icon = icon;
    }

    public void Start(Enemy enemy)
    {
        _enemy = enemy;
        _burnRoutine = _enemy.StartCoroutine(BurnRoutine());
    }

    public void Refresh(IStatusEffect newEffect)
    {
        if (newEffect is BurnDebuff burn)
        {
            Duration = Mathf.Max(Duration, burn.Duration);
            DPS = Mathf.Max(DPS, burn.DPS);
            Elapsed = 0f; // обновление таймера
        }
    }

    public void Stop()
    {
        if (_burnRoutine != null && _enemy != null)
        {
            _enemy.StopCoroutine(_burnRoutine);
            _burnRoutine = null;
        }
    }

    private IEnumerator BurnRoutine()
    {
        while (Elapsed < Duration && _enemy.IsAlive)
        {
            _enemy.TakeDamage(DPS * Time.deltaTime);

            Elapsed += Time.deltaTime;
            yield return null;
        }

        Stop();
        _enemy.RemoveStatusEffect(EffectType);
    }
}
