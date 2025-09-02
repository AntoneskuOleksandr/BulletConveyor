using System.Collections.Generic;
using UnityEngine;

public class ModifierManager : MonoBehaviour
{
    private Enemy _enemy;
    private EnemyStatusCanvas _enemyStatusCanvas;

    private Dictionary<OnHitEffectType, IStatusEffect> _effects = new();
    private List<IEffectRule> _rules = new();

    public void Initialize(Enemy enemy, EnemyStatusCanvas enemyStatusCanvas)
    {
        _enemy = enemy;
        _enemyStatusCanvas = enemyStatusCanvas;

        // Регистрируем правила взаимодействий эффектов
        _rules.Add(new FreezeToIceRule());
        _rules.Add(new FireMeltsFreezeRule());
        _rules.Add(new FireMeltsIceRule());
        _rules.Add(new FreezeRefreshesIceRule());
    }

    public void AddStatusEffect(IStatusEffect effect)
    {
        Debug.Log("AddStatusEffect: " + effect);

        if (_effects.TryGetValue(effect.EffectType, out var existing))
        {
            existing.Refresh(effect);
        }
        else
        {
            _effects[effect.EffectType] = effect;
            _enemyStatusCanvas.AddStatusEffect(effect);
        }

        HandleEffectCombinations();
    }

    public void RemoveStatusEffect(OnHitEffectType type)
    {
        if (_effects.TryGetValue(type, out var effect))
        {
            _effects.Remove(type);
            _enemyStatusCanvas.RemoveStatusEffect(type);

            HandleEffectCombinations();
        }
    }

    public bool HasStatusEffect(OnHitEffectType type) => _effects.ContainsKey(type);

    private void Update()
    {
        if (_enemy == null || !_enemy.IsAlive)
            return;

        float speedModifier = 1f;
        List<OnHitEffectType> toRemove = new List<OnHitEffectType>();

        foreach (var kv in _effects)
        {
            var effect = kv.Value;

            // Tick для эффектов с временем действия
            if (effect is ITickEffect tick)
            {
                tick.Tick(Time.deltaTime, _enemy);

                if (tick.IsFinished())
                    toRemove.Add(kv.Key);
            }

            // Применяем модификаторы (например, замедление)
            if (effect is IModifierEffect mod)
            {
                var modifiers = mod.GetModifiers();
                if (modifiers.TryGetValue(ModifierType.Slow, out float slow))
                    speedModifier *= 1f - slow;
            }
        }

        // Удаляем эффекты, которые закончились
        foreach (var type in toRemove)
            RemoveStatusEffect(type);

        // Применяем итоговое замедление
        _enemy.SetSpeed(_enemy.BaseSpeed * speedModifier);
    }

    private void HandleEffectCombinations()
    {
        foreach (var rule in _rules)
            rule.Evaluate(_effects, this);
    }
}
