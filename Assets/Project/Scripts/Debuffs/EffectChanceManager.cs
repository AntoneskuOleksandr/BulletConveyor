using System.Collections.Generic;
using UnityEngine;

public class EffectChanceManager
{
    /// <summary>
    /// Глобальные шансы (применяются для всех оружий)
    /// </summary>
    private readonly List<ProjectileEffectChance> _globalChances = new List<ProjectileEffectChance>();

    /// <summary>
    /// Шансы для конкретных оружий
    /// </summary>
    private readonly Dictionary<Weapon, List<ProjectileEffectChance>> _weaponChances
        = new Dictionary<Weapon, List<ProjectileEffectChance>>();

    // ----------------- ПУБЛИЧНЫЕ МЕТОДЫ -----------------

    /// <summary>
    /// Добавить шанс глобально (применяется для всех оружий)
    /// </summary>
    public void RegisterGlobalChance(OnHitEffectType type, float value, ChanceSource source = ChanceSource.Base)
    {
        RegisterChanceInternal(_globalChances, type, value, source);
    }

    /// <summary>
    /// Добавить шанс для конкретного оружия
    /// </summary>
    public void RegisterWeaponChance(Weapon weapon, OnHitEffectType type, float value)
    {
        if (!_weaponChances.TryGetValue(weapon, out var list))
        {
            list = new List<ProjectileEffectChance>();
            _weaponChances[weapon] = list;
        }
        RegisterChanceInternal(list, type, value, ChanceSource.Weapon);
    }

    /// <summary>
    /// Сбросить все шансы (например, при рестарте уровня)
    /// </summary>
    public void ClearAll()
    {
        _globalChances.Clear();
        _weaponChances.Clear();
    }

    /// <summary>
    /// Получить список эффектов, которые выпали, для конкретного оружия.
    /// Складывает глобальные шансы + шансы оружия.
    /// </summary>
    public List<OnHitEffectType> RollEffectsForWeapon(Weapon weapon)
    {
        var results = new List<OnHitEffectType>();

        // Собрать все шансы (глобальные + локальные)
        var allChances = new List<ProjectileEffectChance>();
        allChances.AddRange(_globalChances);

        if (weapon != null && _weaponChances.TryGetValue(weapon, out var weaponList))
            allChances.AddRange(weaponList);

        foreach (var chance in allChances)
        {
            if (Random.value < chance.TotalChance)
                results.Add(chance.EffectType);
        }

        return results;
    }

    /// <summary>
    /// Удалить все бонусы для конкретного оружия (например, при его продаже)
    /// </summary>
    public void ClearWeaponChances(Weapon weapon)
    {
        if (_weaponChances.ContainsKey(weapon))
            _weaponChances.Remove(weapon);
    }

    // ----------------- ВСПОМОГАТЕЛЬНЫЕ -----------------

    private void RegisterChanceInternal(List<ProjectileEffectChance> list, OnHitEffectType type, float value, ChanceSource source)
    {
        var existing = list.Find(c => c.EffectType == type);
        if (existing == null)
        {
            existing = new ProjectileEffectChance { EffectType = type };
            list.Add(existing);
        }
        existing.AddModifier(source, value);
    }

    // ----------------- ВНУТРЕННИЕ ТИПЫ -----------------

    public enum ChanceSource
    {
        Base,       // Базовый шанс (например, от выбора абилки)
        Weapon,     // Бонус от оружия
        Buff,       // Бонус от баффа
        Event,      // Бонус от события
        Perk,       // Бонус от постоянного перка
        Other       // На будущее
    }

    [System.Serializable]
    public class ProjectileEffectChance
    {
        public OnHitEffectType EffectType;
        private readonly Dictionary<ChanceSource, float> _modifiers = new Dictionary<ChanceSource, float>();

        public void AddModifier(ChanceSource source, float value)
        {
            if (_modifiers.ContainsKey(source))
                _modifiers[source] += value;
            else
                _modifiers[source] = value;
        }

        public void SetModifier(ChanceSource source, float value)
        {
            _modifiers[source] = value;
        }

        public float GetModifier(ChanceSource source)
        {
            return _modifiers.TryGetValue(source, out var val) ? val : 0f;
        }

        public float TotalChance
        {
            get
            {
                float sum = 0f;
                foreach (var kvp in _modifiers)
                    sum += kvp.Value;
                return Mathf.Clamp01(sum);
            }
        }
    }
}
