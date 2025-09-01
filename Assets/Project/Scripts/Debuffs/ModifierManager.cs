using System.Collections.Generic;
using UnityEngine;

public class ModifierManager : MonoBehaviour
{
    private Enemy _enemy;
    private EnemyStatusCanvas _enemyStatusCanvas;

    private Dictionary<OnHitEffectType, IStatusEffect> _effects = new();
    private HashSet<string> _activeCombinations = new HashSet<string>();

    public void Initialize(Enemy enemy, EnemyStatusCanvas enemyStatusCanvas)
    {
        _enemy = enemy;
        _enemyStatusCanvas = enemyStatusCanvas;
    }

    public void AddStatusEffect(IStatusEffect effect)
    {
        Debug.Log("AddStatusEffect: " + effect);

        // Обновляем существующий эффект или добавляем новый
        if (_effects.TryGetValue(effect.EffectType, out var existing))
        {
            existing.Refresh(effect);
        }
        else
        {
            _effects[effect.EffectType] = effect;
            _enemyStatusCanvas.AddStatusEffect(effect, effect.Icon);
        }

        // Проверяем комбинации после добавления
        HandleEffectCombinations();
    }

    public void RemoveStatusEffect(OnHitEffectType type)
    {
        if (_effects.TryGetValue(type, out var effect))
        {
            _effects.Remove(type);
            _enemyStatusCanvas.RemoveStatusEffect(type);
        }
    }

    private void Update()
    {
        if (_enemy == null || !_enemy.IsAlive)
            return;

        float speedModifier = 1f;
        List<OnHitEffectType> toRemove = new List<OnHitEffectType>();

        // Tick всех эффектов
        foreach (var kv in _effects)
        {
            var effect = kv.Value;

            if (effect is ITickEffect tick)
            {
                tick.Tick(Time.deltaTime, _enemy);

                if (tick.IsFinished())
                    toRemove.Add(kv.Key);
            }

            if (effect is IModifierEffect mod)
            {
                var modifiers = mod.GetModifiers();
                if (modifiers.TryGetValue(ModifierType.Slow, out float slow))
                    speedModifier *= 1f - slow;
            }
        }

        // Удаляем завершившиеся эффекты
        foreach (var type in toRemove)
            RemoveStatusEffect(type);

        // Применяем замедление к врагу
        _enemy.SetSpeed(_enemy.BaseSpeed * speedModifier);
    }

    /// <summary>
    /// Обрабатывает комбинации эффектов и их триггеры
    /// </summary>
    private void HandleEffectCombinations()
    {
        // Пример: Freeze + Fire = Melt
        FreezeDebuff freezeDebuff = null;
        ITickEffect fireEffect = null;

        bool hasFreeze = _effects.TryGetValue(OnHitEffectType.Freeze, out var freezeObj) && (freezeDebuff = freezeObj as FreezeDebuff) != null;
        bool hasFire = _effects.TryGetValue(OnHitEffectType.Fire, out var fireObj) && (fireEffect = fireObj as ITickEffect) != null;

        string comboKey = "Freeze+Fire";

        if (hasFreeze && hasFire)
        {
            if (!_activeCombinations.Contains(comboKey))
            {
                freezeDebuff.StartMelt(2f); // ускоряем таяние
                _activeCombinations.Add(comboKey);
            }
        }
        else
        {
            // Комбинация больше не выполняется
            _activeCombinations.Remove(comboKey);
        }

        // Здесь можно добавить другие комбинации, например:
        // Freeze + Poison, Fire + Poison и т.д.
    }
}
