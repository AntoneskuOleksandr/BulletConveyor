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

    private LightningVisual _visual;

    public LightningDebuff(float damage, float radius, int maxBounces, float duration, float ticksPerSecond = 2f, LightningVisual visual = null)
    {
        _damage = damage;
        _radius = radius;
        _maxBounces = maxBounces;
        Duration = duration;
        _ticksPerSecond = Mathf.Max(0.01f, ticksPerSecond);
        Elapsed = 0f;
        _nextTickTime = 0f;
        _visual = visual; // визуализация
    }

    private void ChainLightning(Enemy sourceEnemy, Vector3 startPosition, float damage, int bouncesLeft, HashSet<Enemy> hitEnemies, List<Vector3> path)
    {
        Enemy next = FindRandomNearbyEnemy(startPosition, hitEnemies, 3); // берём случайного из 3 ближайших
        if (next == null) return;

        next.TakeDamage(damage);
        hitEnemies.Add(next);
        path.Add(next.HitTransform.position);

        if (bouncesLeft > 0)
        {
            float nextDamage = damage * DEFAULT_DAMAGE_FALLOFF;
            ChainLightning(sourceEnemy, next.HitTransform.position, nextDamage, bouncesLeft - 1, hitEnemies, path);
        }
    }

    public void Tick(float deltaTime, Enemy enemy)
    {
        Elapsed += deltaTime;

        float tickInterval = 1f / _ticksPerSecond;

        while (_nextTickTime <= Elapsed && _nextTickTime < Duration)
        {
            _nextTickTime += tickInterval;

            HashSet<Enemy> hitEnemies = new HashSet<Enemy>();
            List<Vector3> path = new List<Vector3> { enemy.HitTransform.position }; // стартовая точка
            ChainLightning(enemy, enemy.HitTransform.position, _damage, _maxBounces, hitEnemies, path);

            // визуализируем молнию
            if (_visual != null && path.Count > 1)
                _visual.DrawChain(path);
        }
    }

    /// <summary>
    /// Находит случайного врага из N ближайших, который ещё не был поражён
    /// </summary>
    private Enemy FindRandomNearbyEnemy(Vector3 position, HashSet<Enemy> exclude, int nearestCount)
    {
        Collider[] colliders = Physics.OverlapSphere(position, _radius);
        List<(Enemy enemy, float dist)> candidates = new List<(Enemy, float)>();

        foreach (var col in colliders)
        {
            if (!col.TryGetComponent<Enemy>(out Enemy enemy) || !enemy.IsAlive || exclude.Contains(enemy))
                continue;

            float dist = Vector3.Distance(position, enemy.HitTransform.position);
            candidates.Add((enemy, dist));
        }

        if (candidates.Count == 0) return null;

        // сортируем по расстоянию и берём N ближайших
        candidates.Sort((a, b) => a.dist.CompareTo(b.dist));
        int count = Mathf.Min(nearestCount, candidates.Count);

        // случайный выбор из N ближайших
        int index = Random.Range(0, count);
        return candidates[index].enemy;
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