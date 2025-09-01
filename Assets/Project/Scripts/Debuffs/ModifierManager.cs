using System.Collections.Generic;
using UnityEngine;

public class ModifierManager : MonoBehaviour
{
    private Enemy _enemy;
    private Dictionary<OnHitEffectType, IStatusEffect> _effects = new();

    public void Initialize(Enemy enemy)
    {
        _enemy = enemy;
    }

    public void AddStatusEffect(IStatusEffect effect)
    {
        if (_effects.TryGetValue(effect.EffectType, out var existing))
        {
            existing.Refresh(effect);
        }
        else
        {
            _effects[effect.EffectType] = effect;
            effect.Start(_enemy);
        }
    }

    public void RemoveStatusEffect(OnHitEffectType type)
    {
        if (_effects.TryGetValue(type, out var effect))
        {
            effect.Stop();
            _effects.Remove(type);
        }
    }

    private void Update()
    {
        float totalDPS = 0f;
        float speedModifier = 1f;

        foreach (var effect in _effects.Values)
        {
            if (effect is ITickEffect tick)
                totalDPS += tick.DPS;

            if (effect is IModifierEffect mod)
            {
                var modifiers = mod.GetModifiers();
                if (modifiers.TryGetValue(ModifierType.Slow, out float slow))
                    speedModifier *= 1f - slow;
            }
        }

        if (totalDPS > 0)
            _enemy.TakeDamage(totalDPS * Time.deltaTime);

        _enemy.SetSpeed(_enemy.BaseSpeed * speedModifier);
    }
}
