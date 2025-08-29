using System.Collections;
using UnityEngine;
using static EffectChanceManager;

public class BurnDebuff : IDebuff
{
    private Enemy _enemy;
    private float _duration;
    private float _dps;

    public ProjectileEffectType Type => ProjectileEffectType.Fire;

    public BurnDebuff(Enemy enemy, IProjectileEffect effect, Bullet bullet)
    {
        _enemy = enemy;
        _duration = effect.Duration;
        _dps = effect.Value;
    }

    public void Start() => _enemy.StartCoroutine(ApplyRoutine());

    public void Refresh(IProjectileEffect effect)
    {
        _duration = Mathf.Max(_duration, effect.Duration); // Продлеваем эффект
        _dps = Mathf.Max(_dps, effect.Value); // Усиливаем при необходимости
    }

    private IEnumerator ApplyRoutine()
    {
        float elapsed = 0f;
        while (elapsed < _duration && _enemy.IsAlive)
        {
            _enemy.TakeDamage(_dps * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }
    }
}
