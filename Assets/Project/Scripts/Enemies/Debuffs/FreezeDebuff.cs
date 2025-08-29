using System.Collections;
using UnityEngine;

public class FreezeDebuff : IStatusEffect
{
    public OnHitEffectType EffectType => OnHitEffectType.Freeze;

    private float _slowAmount;      // базовое замедление
    private float _duration;        // базовое врем€ действи€
    private float _stackIncrement;  // на сколько увеличиваетс€ замедление при повторном попадании

    private float _currentSlow;     // текущий эффект замедлени€
    private float _timeLeft;

    private Coroutine _routine;

    public FreezeDebuff(float slowAmount, float duration, float stackIncrement)
    {
        _slowAmount = slowAmount;
        _duration = duration;
        _stackIncrement = stackIncrement;
        _currentSlow = slowAmount;
        _timeLeft = duration;
    }

    public void Start(Enemy enemy)
    {
        enemy.ModifySpeed(1f - _currentSlow); // сразу замедл€ем
        _routine = enemy.StartCoroutine(FreezeRoutine(enemy));
    }

    public void Refresh(IStatusEffect newEffect)
    {
        if (newEffect is FreezeDebuff freeze)
        {
            _timeLeft = Mathf.Max(_timeLeft, freeze._duration);          // продлеваем таймер
            _currentSlow = Mathf.Min(_currentSlow + freeze._stackIncrement, 1f); // накапливаем эффект
        }
    }

    public void Stop(Enemy enemy)
    {
        if (_routine != null) 
        {
            enemy.StopCoroutine(_routine);
            _routine = null;
        }
        enemy.ModifySpeed(1f); // восстанавливаем скорость
    }

    private IEnumerator FreezeRoutine(Enemy enemy)
    {
        while (_timeLeft > 0f && enemy.IsAlive)
        {
            // ќбновл€ем скорость каждый кадр (можно сделать плавную интерпол€цию)
            enemy.ModifySpeed(1f - _currentSlow);

            _timeLeft -= Time.deltaTime;
            yield return null;
        }

        Stop(enemy);
        enemy.RemoveStatusEffect(EffectType);
    }
}
