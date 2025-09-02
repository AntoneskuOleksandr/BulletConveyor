using System.Collections.Generic;
using UnityEngine;

public class LightningDebuff : ITickEffect
{
    public OnHitEffectType EffectType => OnHitEffectType.Lightning;
    public float Duration { get; private set; }
    public float Elapsed { get; private set; }
    public float DPS => 0f;
    public float DurationLeft => Mathf.Max(Duration - Elapsed, 0f);

    private float _damage;
    private float _radius;
    private int _maxBounces;
    private float _ticksPerSecond;
    private float _nextTickTime;

    private const float DEFAULT_DAMAGE_FALLOFF = 1f;

    public LightningDebuff(float damage, float radius, int maxBounces, float duration, float ticksPerSecond = 2f)
    {
        _damage = damage;
        _radius = radius;
        _maxBounces = maxBounces;
        Duration = duration;
        _ticksPerSecond = Mathf.Max(0.01f, ticksPerSecond);
        Elapsed = 0f;
        _nextTickTime = 0f; // первый удар сразу
    }

    public void Tick(float deltaTime, Enemy enemy)
    {
        Elapsed += deltaTime;

        float tickInterval = 1f / _ticksPerSecond;

        // наносим все тики, которые должны были произойти за это deltaTime
        while (_nextTickTime <= Elapsed && _nextTickTime < Duration)
        {
            HashSet<Enemy> hitEnemies = new HashSet<Enemy>();
            ChainLightning(enemy, enemy.transform.position, _damage, _maxBounces, hitEnemies);

            _nextTickTime += tickInterval;
        }
    }

    private void ChainLightning(Enemy sourceEnemy, Vector3 startPosition, float damage, int bouncesLeft, HashSet<Enemy> hitEnemies)
    {
        Enemy nearest = FindNearestEnemy(startPosition, hitEnemies);
        if (nearest == null) return;

        nearest.TakeDamage(damage);
        hitEnemies.Add(nearest);

        Debug.DrawLine(startPosition, nearest.transform.position, Color.yellow, 0.1f);

        if (bouncesLeft > 0)
        {
            float nextDamage = damage * DEFAULT_DAMAGE_FALLOFF;
            ChainLightning(sourceEnemy, nearest.transform.position, nextDamage, bouncesLeft - 1, hitEnemies);
        }
    }

    private Enemy FindNearestEnemy(Vector3 position, HashSet<Enemy> exclude)
    {
        Collider[] colliders = Physics.OverlapSphere(position, _radius);
        Enemy nearest = null;
        float minDistance = float.MaxValue;

        foreach (var col in colliders)
        {
            if (!col.TryGetComponent<Enemy>(out Enemy enemy) || !enemy.IsAlive || exclude.Contains(enemy))
                continue;

            float dist = Vector3.Distance(position, enemy.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                nearest = enemy;
            }
        }

        return nearest;
    }

    public void Refresh(IStatusEffect newEffect)
    {
        if (newEffect is LightningDebuff lightning)
        {
            Duration = Mathf.Max(Duration, lightning.Duration);
            Elapsed = 0f;
            _damage = Mathf.Max(_damage, lightning._damage);
            _nextTickTime = 0f; // сброс для мгновенного первого удара
        }
    }

    public bool IsFinished() => Elapsed >= Duration;
}
