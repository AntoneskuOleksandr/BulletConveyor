using System.Collections;
using UnityEngine;

public class BurnDebuff : IStatusEffect
{
    public OnHitEffectType EffectType => OnHitEffectType.Fire;

    private float _duration;
    private float _dps;
    private float _timeLeft;

    private Coroutine _burnRoutine;

    public BurnDebuff(float duration, float dps)
    {
        _duration = duration;
        _dps = dps;
        _timeLeft = duration;
    }

    public void Start(Enemy enemy)
    {
        // Запускаем корутину горения
        _burnRoutine = enemy.StartCoroutine(BurnRoutine(enemy));
    }

    public void Refresh(IStatusEffect newEffect)
    {
        if (newEffect is BurnDebuff burn)
        {
            // Продлеваем время, можно усиливать урон, если нужно
            _timeLeft = Mathf.Max(_timeLeft, burn._duration);
            _dps = Mathf.Max(_dps, burn._dps);
        }
    }

    public void Stop()
    {
        // Если нужно прерывать — останавливаем корутину
        // (Враг сам вызывает Stop() при удалении эффекта)
    }

    private IEnumerator BurnRoutine(Enemy enemy)
    {
        while (_timeLeft > 0f && enemy.IsAlive)
        {
            enemy.TakeDamage(_dps * Time.deltaTime);
            _timeLeft -= Time.deltaTime;
            yield return null;
        }

        enemy.RemoveStatusEffect(EffectType);
    }
}
