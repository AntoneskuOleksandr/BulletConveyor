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
        _burnRoutine = enemy.StartCoroutine(BurnRoutine(enemy));
    }

    public void Refresh(IStatusEffect newEffect)
    {
        if (newEffect is BurnDebuff burn)
        {
            _timeLeft = Mathf.Max(_timeLeft, burn._duration);
            _dps = Mathf.Max(_dps, burn._dps);
        }
    }

    public void Stop(Enemy enemy)
    {
        if (_burnRoutine != null)
        {
            enemy.StopCoroutine(_burnRoutine);
            _burnRoutine = null;
        }
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
