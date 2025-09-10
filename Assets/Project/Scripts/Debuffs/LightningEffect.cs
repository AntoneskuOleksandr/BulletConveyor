using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Ёффект молнии, вызываемый при попадании пули.
/// —троит цепочку врагов (target + ближайшие), вызывает визуал и накладывает LightningDebuff на каждого.
/// </summary>
public class LightningEffect : IOnHitEffect
{
    public OnHitEffectType EffectType => OnHitEffectType.Lightning;

    private float _damage;
    private float _radius;
    private int _maxBounces;
    private float _slowAmount;
    private float _duration;
    private LightningManager _lightningManager;

    public LightningEffect(StatusEffectSO so, LightningManager lightningManager)
    {
        // безопасные чтени€ параметров Ч если параметра нет, GetParameter может вернуть 0
        _damage = so != null ? so.GetParameter(EffectParameterType.Damage) : 0f;
        _radius = so != null ? Mathf.Max(0.1f, so.GetParameter(EffectParameterType.Radius)) : 3f;
        _maxBounces = so != null ? Mathf.Max(0, (int)so.GetParameter(EffectParameterType.MaxBounces)) : 2;
        _slowAmount = so != null ? Mathf.Clamp01(so.GetParameter(EffectParameterType.SlowAmount)) : 0.25f;
        _duration = so != null ? Mathf.Max(0.01f, so.Duration) : 1f;
        _lightningManager = lightningManager;
    }

    public void Apply(Bullet bullet, Enemy initialTarget)
    {
        if (initialTarget == null) return;

        // 1) собираем цепочку врагов
        var chain = BuildLightningChain(initialTarget);

        if (chain.Count < 1) return;

        // 2) визуал (через менеджер)

        _lightningManager?.DrawChain(chain);

        // 3) накладываем дебафф на каждого врага в цепочке
        foreach (var e in chain)
        {
            if (e == null || !e.IsAlive) continue;
            var debuff = new LightningDebuff(_damage, _slowAmount, _duration);
            e.ModifierManager.AddStatusEffect(debuff);
        }
    }

    private List<Enemy> BuildLightningChain(Enemy start)
    {
        var list = new List<Enemy> { start };
        var current = start;

        for (int i = 0; i < _maxBounces; i++)
        {
            var next = FindClosestEnemy(current, list);
            if (next == null) break;
            list.Add(next);
            current = next;
        }

        return list;
    }

    private Enemy FindClosestEnemy(Enemy from, List<Enemy> exclude)
    {
        Collider[] hits = Physics.OverlapSphere(from.transform.position, _radius);
        float bestDist = float.MaxValue;
        Enemy best = null;

        foreach (var c in hits)
        {
            if (c == null) continue;
            if (!c.TryGetComponent<Enemy>(out var e)) continue;
            if (!e.IsAlive) continue;
            if (exclude.Contains(e)) continue;

            float d = Vector3.Distance(from.transform.position, e.transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                best = e;
            }
        }

        return best;
    }
}
