using System.Collections;
using UnityEngine;
using static ModifierManager;

public class BurnDebuff : IStatusEffect
{
    public OnHitEffectType EffectType => OnHitEffectType.Fire;

    private float _duration;
    private float _dps;
    private float _timeLeft;

    private Coroutine _burnRoutine;
    private Enemy _enemy;

    public BurnDebuff(float duration, float dps)
    {
        _duration = duration;
        _dps = dps;
        _timeLeft = duration;
    }

    public void Start(Enemy enemy)
    {
        _enemy = enemy;

        // Добавляем модификатор урона от огня
        _enemy.ModifierManager.AddModifier(ModifierType.FireDamageTaken, 1f); // 1.0 = без изменения, можно ставить 1.2, 1.5 и т.д.

        _burnRoutine = _enemy.StartCoroutine(BurnRoutine());
    }

    public void Refresh(IStatusEffect newEffect)
    {
        if (newEffect is BurnDebuff burn)
        {
            _timeLeft = Mathf.Max(_timeLeft, burn._duration);
            _dps = Mathf.Max(_dps, burn._dps);
        }
    }

    public void Stop()
    {
        if (_burnRoutine != null && _enemy != null)
        {
            _enemy.StopCoroutine(_burnRoutine);
            _burnRoutine = null;
        }

        // Убираем модификатор
        _enemy.ModifierManager.RemoveModifier(ModifierType.FireDamageTaken);
    }

    private IEnumerator BurnRoutine()
    {
        while (_timeLeft > 0f && _enemy.IsAlive)
        {
            float damage = _dps * Time.deltaTime;
            damage = _enemy.ModifierManager.ApplyModifiers(ModifierType.FireDamageTaken, damage);

            _enemy.TakeDamage(damage);

            _timeLeft -= Time.deltaTime;
            yield return null;
        }

        Stop();
        _enemy.RemoveStatusEffect(EffectType);
    }
}
