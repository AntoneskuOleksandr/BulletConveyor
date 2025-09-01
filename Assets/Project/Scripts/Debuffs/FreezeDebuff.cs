using System.Collections;
using UnityEngine;

public class FreezeDebuff : IStatusEffect
{
    public OnHitEffectType EffectType => OnHitEffectType.Freeze;

    public float DurationLeft => throw new System.NotImplementedException();

    private float _duration;
    private float _slowAmount;      // максимальное замедление (например 0.1f = 10% скорости)
    private float _timeLeft;

    private Coroutine _freezeRoutine;
    private Enemy _enemy;

    public FreezeDebuff(float duration, float slowAmount)
    {
        _duration = duration;
        _slowAmount = Mathf.Clamp01(slowAmount);
        _timeLeft = duration;
    }

    public void Start(Enemy enemy)
    {
        _enemy = enemy;

        // Запускаем корутину заморозки
        _freezeRoutine = _enemy.StartCoroutine(FreezeRoutine());
    }

    public void Refresh(IStatusEffect newEffect)
    {
        if (newEffect is FreezeDebuff freeze)
        {
            // Продлеваем эффект
            _timeLeft = Mathf.Max(_timeLeft, freeze._duration);
            _slowAmount = Mathf.Max(_slowAmount, freeze._slowAmount);
        }
    }

    public void Stop()
    {
        if (_freezeRoutine != null && _enemy != null)
        {
            _enemy.StopCoroutine(_freezeRoutine);
            _freezeRoutine = null;
        }
    }

    private IEnumerator FreezeRoutine()
    {
        while (_timeLeft > 0f && _enemy.IsAlive)
        {
            // Чем меньше времени осталось, тем сильнее замедление
            float multiplier = Mathf.Lerp(1f, 1f - _slowAmount, 1f - (_timeLeft / _duration));

            _timeLeft -= Time.deltaTime;
            yield return null;
        }

        Stop();
        _enemy.RemoveStatusEffect(EffectType);
    }
}
