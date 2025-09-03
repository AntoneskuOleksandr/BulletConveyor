using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class ModifierManager : MonoBehaviour
{
    private Enemy _enemy;
    private EnemyStatusCanvas _enemyStatusCanvas;

    // Словарь: тип эффекта -> список экземпляров эффектов
    private Dictionary<OnHitEffectType, List<IStatusEffect>> _effects = new();
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
                    list[0].Refresh(effect); // обновляем существующий
                }
                else
                {
                    list.Add(effect);
                    _enemyStatusCanvas.AddStatusEffect(effect);
                }
                break;

            case EffectStackType.Stackable:
                list.Add(effect);
                _enemyStatusCanvas.AddStatusEffect(effect);
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
            list.Remove(effect);
            _enemyStatusCanvas.RemoveStatusEffect(effect);

            if (list.Count == 0)
                _effects.Remove(effect.EffectType);

            HandleEffectCombinations();
        }
    }

    public bool HasStatusEffect(OnHitEffectType type) => _effects.ContainsKey(type);

    private void Update()
    {
        if (_enemy == null || !_enemy.IsAlive)
            return;

        float speedModifier = 1f;
        List<IStatusEffect> toRemove = new List<IStatusEffect>();

        foreach (var kv in _effects)
        {
            foreach (var effect in kv.Value)
            {
                // Tick для эффектов с временем действия
                if (effect is ITickEffect tick)
                {
                    tick.Tick(Time.deltaTime, _enemy);
                    if (tick.IsFinished())
                        toRemove.Add(effect);
                }

                // Применяем модификаторы (например, замедление)
                if (effect is IModifierEffect mod)
                {
                    var modifiers = mod.GetModifiers();
                    if (modifiers.TryGetValue(ModifierType.Slow, out float slow))
                        speedModifier *= 1f - slow;
                }
            }
        }

        // Удаляем эффекты, которые закончились
        foreach (var effect in toRemove)
            RemoveStatusEffect(effect);

        // Применяем итоговое замедление
        _enemy.SetSpeed(_enemy.BaseSpeed * speedModifier);
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
