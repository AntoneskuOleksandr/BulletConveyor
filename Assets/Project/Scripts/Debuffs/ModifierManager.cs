using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class ModifierManager : MonoBehaviour
{
    private Enemy _enemy;
    private EnemyStatusCanvas _enemyStatusCanvas;

    // Активные эффекты: Fire, Ice и т.д.
    private readonly Dictionary<OnHitEffectType, List<IStatusEffect>> _effects = new();

    // Только тикеры (для оптимизации)
    private readonly List<ITickableEffect> _tickables = new();

    // Только модификаторы (для быстрого пересчета статы)
    private readonly List<IModifierEffect> _modifiers = new();

    // Кэш итоговых модификаторов
    private readonly Dictionary<ModifierType, float> _modifierTotals = new();

    // Dirty-флаг: нужно ли пересчитать модификаторы
    private bool _modifiersDirty = true;

    private List<IEffectRule> _rules = new();

    [Inject] private EffectRulesConfigSO _config;

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

    /// <summary>
    /// Добавляем эффект
    /// </summary>
    public void AddStatusEffect(IStatusEffect effect)
    {
        // Instant-эффекты выполняются сразу и не сохраняются
        if (effect is IInstantEffect instant)
        {
            instant.Apply(_enemy);
            return;
        }

        if (!_effects.TryGetValue(effect.EffectType, out var list))
        {
            list = new List<IStatusEffect>();
            _effects[effect.EffectType] = list;
        }

        switch (effect.StackType)
        {
            case EffectStackType.Refreshable:
                if (list.Count > 0)
                {
                    list[0].Refresh(effect);
                }
                else
                {
                    list.Add(effect);
                    _enemyStatusCanvas.AddStatusEffect(effect);
                    RegisterEffect(effect);
                }
                break;

            case EffectStackType.Stackable:
                list.Add(effect);
                _enemyStatusCanvas.AddStatusEffect(effect);
                RegisterEffect(effect);
                break;
        }

        HandleEffectCombinations();
    }

    /// <summary>
    /// Удаляем эффект
    /// </summary>
    public void RemoveStatusEffect(IStatusEffect effect)
    {
        if (_effects.TryGetValue(effect.EffectType, out var list))
        {
            if (list.Remove(effect))
            {
                _enemyStatusCanvas.RemoveStatusEffect(effect);
                UnregisterEffect(effect);

                if (list.Count == 0)
                    _effects.Remove(effect.EffectType);

                HandleEffectCombinations();
            }
        }
    }

    public bool HasStatusEffect(OnHitEffectType type) => _effects.ContainsKey(type);

    private void Update()
    {
        if (_enemy == null || !_enemy.IsAlive)
            return;

        float deltaTime = Time.deltaTime;
        List<IStatusEffect> toRemove = null;

        // Тикаем только нужные эффекты
        foreach (var tickable in _tickables)
        {
            tickable.Tick(deltaTime, _enemy);
            if (tickable.IsFinished)
            {
                toRemove ??= new List<IStatusEffect>();
                toRemove.Add(tickable);
            }
        }

        // Удаляем законченные эффекты
        if (toRemove != null)
        {
            foreach (var eff in toRemove)
                RemoveStatusEffect(eff);
        }

        // Применяем модификаторы, если были изменения
        if (_modifiersDirty)
        {
            RecalculateModifiers();
            _modifiersDirty = false;
        }

        // Применяем итоговую скорость
        float speed = _enemy.BaseSpeed;
        if (_modifierTotals.TryGetValue(ModifierType.Slow, out float slow))
        {
            speed *= 1f - slow;
        }

        _enemy.SetSpeed(speed);
    }

    private void RegisterEffect(IStatusEffect effect)
    {
        if (effect is ITickableEffect tickable)
            _tickables.Add(tickable);

        if (effect is IModifierEffect modifier)
        {
            _modifiers.Add(modifier);
            _modifiersDirty = true;
        }
    }

    private void UnregisterEffect(IStatusEffect effect)
    {
        if (effect is ITickableEffect tickable)
            _tickables.Remove(tickable);

        if (effect is IModifierEffect modifier)
        {
            _modifiers.Remove(modifier);
            _modifiersDirty = true;
        }
    }

    private void RecalculateModifiers()
    {
        _modifierTotals.Clear();

        foreach (var mod in _modifiers)
        {
            var dict = mod.GetModifiers();
            foreach (var kv in dict)
            {
                if (_modifierTotals.ContainsKey(kv.Key))
                    _modifierTotals[kv.Key] += kv.Value;
                else
                    _modifierTotals[kv.Key] = kv.Value;
            }
        }
    }

    /// <summary>
    /// Обрабатываем комбинации эффектов
    /// </summary>
    private void HandleEffectCombinations()
    {
        foreach (var rule in _rules)
            rule.Evaluate(_effects, this, _config);
    }
}
