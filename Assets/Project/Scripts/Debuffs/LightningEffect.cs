using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Эффект молнии: визуал всегда от начальной цели, урон и дебафф только живым.
/// </summary>
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

    public void Apply(Bullet bullet, Enemy initialTarget)
    {
        if (initialTarget == null)
        {
            return;
        }

        _lightningManager.StartCoroutine(DoLightningChain(initialTarget));
    }

    private IEnumerator DoLightningChain(Enemy initialTarget)
    {
        float elapsed = 0f;
        float tickInterval = 1f / _ticksPerSecond;

        bool firstTick = true;
        Enemy currentRoot = initialTarget.IsAlive ? initialTarget : FindClosestEnemyToPoint(initialTarget.HitTransform.position, null);

        while (elapsed < _duration)
        {
            List<Enemy> chain = new List<Enemy>();

            if (firstTick)
            {
                // На первом тике рисуем initialTarget для визуала
                chain.Add(initialTarget);
                firstTick = false;
            }

            // Достраиваем цепочку из живых врагов
            if (currentRoot != null)
            {
                var additionalChain = BuildLightningChain(currentRoot);

                // Не дублируем initialTarget
                additionalChain.Remove(initialTarget);
                chain.AddRange(additionalChain);

                // Наносим урон и дебафф только живым
                foreach (var enemy in additionalChain)
                {
                    if (enemy == null || !enemy.IsAlive) continue;

                    enemy.TakeDamage(_dps / _ticksPerSecond);
                    enemy.ModifierManager.AddStatusEffect(new LightningDebuff(_slowAmount, _debuffDuration));
                }

                // Для следующего тика
                currentRoot = additionalChain.Count > 0 ? additionalChain[0] : FindClosestEnemyToPoint(initialTarget.HitTransform.position, null);
            }

            if (chain.Count >= 2)
                _lightningManager.DrawChain(chain);

            if (currentRoot == null && chain.Count <= 1)
                yield break;

            yield return new WaitForSeconds(tickInterval);
            elapsed += tickInterval;
        }
    }

    private List<Enemy> BuildLightningChain(Enemy start)
    {
        List<Enemy> chain = new List<Enemy>();
        if (start == null || !start.IsAlive) return chain;

        chain.Add(start);
        Enemy current = start;

        for (int i = 0; i < _maxBounces; i++)
        {
            Enemy next = FindClosestEnemyToPoint(current.HitTransform.position, chain);
            if (next == null) break;
            chain.Add(next);
            current = next;
        }

        return chain;
    }

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
