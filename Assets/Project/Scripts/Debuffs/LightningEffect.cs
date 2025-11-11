using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningEffect : IOnHitEffect
{
    public OnHitEffectType EffectType => OnHitEffectType.Lightning;

    private readonly float _dps;
    private readonly float _radius;
    private readonly int _maxBounces;
    private readonly float _duration;
    private readonly float _ticksPerSecond;
    private readonly float _slowAmount;
    private readonly float _debuffDuration;
    private readonly LightningManager _lightningManager;

    public LightningEffect(StatusEffectSO so, LightningManager lightningManager)
    {
        _dps = so != null ? so.GetParameter(EffectParameterType.DPS) : 0f;
        _radius = so != null ? Mathf.Max(0.1f, so.GetParameter(EffectParameterType.Radius)) : 3f;
        _maxBounces = so != null ? Mathf.Max(0, (int)so.GetParameter(EffectParameterType.MaxBounces)) : 2;
        _duration = so != null ? Mathf.Max(0.01f, so.Duration) : 1f;
        _ticksPerSecond = so != null ? Mathf.Max(1, so.GetParameter(EffectParameterType.TicksPerSecond)) : 1;

        _slowAmount = so != null ? Mathf.Clamp01(so.GetParameter(EffectParameterType.SlowAmount)) : 0.25f;
        _debuffDuration = so != null ? Mathf.Max(0.01f, so.GetParameter(EffectParameterType.DebuffDuration)) : 1f;

        _lightningManager = lightningManager;
    }

    public void Apply(ProjectileBase projectile, Enemy initialTarget)
    {
        if (initialTarget == null) return;
        _lightningManager.StartCoroutine(DoLightningChain(initialTarget));
    }

    private IEnumerator DoLightningChain(Enemy initialTarget)
    {
        float elapsed = 0f;
        float tickInterval = 1f / _ticksPerSecond;
        float tickDamage = _dps / _ticksPerSecond;

        Vector3 initialPos = initialTarget.HitTransform.position;
        bool isFirstTick = true;

        while (elapsed < _duration)
        {
            List<Enemy> chain = new List<Enemy>();

            if (isFirstTick)
            {
                // На первом тике всегда добавляем initialTarget в визуал
                chain.Add(initialTarget);

                if (initialTarget.IsAlive)
                {
                    initialTarget.TakeDamage(tickDamage);
                    initialTarget.ModifierManager.AddStatusEffect(new LightningDebuff(_slowAmount, _debuffDuration));
                }
            }

            // Определяем стартовую точку для поиска следующего
            Enemy current = (isFirstTick && initialTarget.IsAlive)
                ? initialTarget
                : FindClosestEnemyToPoint(isFirstTick ? initialPos : initialTarget.HitTransform.position, chain);

            for (int i = 0; i < _maxBounces; i++)
            {
                if (current == null) break;

                Enemy next = FindClosestEnemyToPoint(current.HitTransform.position, chain);
                if (next == null) break;

                chain.Add(next);

                if (next.IsAlive)
                {
                    next.TakeDamage(tickDamage);
                    next.ModifierManager.AddStatusEffect(new LightningDebuff(_slowAmount, _debuffDuration));
                }

                current = next;
            }

            if (chain.Count >= 2)
                _lightningManager.DrawChain(chain);

            yield return new WaitForSeconds(tickInterval);
            elapsed += tickInterval;
            isFirstTick = false;
        }
    }

    /// <summary>
    /// Ищет ближайшего живого врага к точке, исключая список exclude.
    /// </summary>
    private Enemy FindClosestEnemyToPoint(Vector3 point, List<Enemy> exclude = null)
    {
        Collider[] hits = Physics.OverlapSphere(point, _radius);
        float bestDist = float.MaxValue;
        Enemy best = null;

        foreach (var c in hits)
        {
            if (c == null) continue;
            if (!c.TryGetComponent<Enemy>(out var enemy)) continue;
            if (!enemy.IsAlive) continue;
            if (exclude != null && exclude.Contains(enemy)) continue;

            float distance = Vector3.Distance(point, enemy.HitTransform.position);
            if (distance < bestDist)
            {
                bestDist = distance;
                best = enemy;
            }
        }

        return best;
    }
}
