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

    public void RegisterGlobalChance(OnHitEffectType type, float value, ChanceSource source = ChanceSource.Base)
    {
        RegisterChanceInternal(_globalChances, type, value, source);
    }

    public void RegisterWeaponChance(Weapon weapon, OnHitEffectType type, float value)
    {
        if (!_weaponChances.TryGetValue(weapon, out var list))
        {
            list = new List<ProjectileEffectChance>();
            _weaponChances[weapon] = list;
        }
        RegisterChanceInternal(list, type, value, ChanceSource.Weapon);
    }

    public void ClearAll()
    {
        _globalChances.Clear();
        _weaponChances.Clear();
    }

    public List<OnHitEffectType> RollEffectsForWeapon(Weapon weapon)
    {
        var results = new List<OnHitEffectType>();

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

    public Dictionary<OnHitEffectType, float> GetAllChancesForWeapon(Weapon weapon)
    {
        var result = new Dictionary<OnHitEffectType, float>();

        foreach (var global in _globalChances)
        {
            result[global.EffectType] = global.TotalChance;
        }

        if (weapon != null && _weaponChances.TryGetValue(weapon, out var weaponList))
        {
            foreach (var local in weaponList)
            {
                if (result.ContainsKey(local.EffectType))
                    result[local.EffectType] += local.TotalChance;
                else
                    result[local.EffectType] = local.TotalChance;
            }
        }

        var keys = new List<OnHitEffectType>(result.Keys);
        foreach (var key in keys)
        {
            result[key] = Mathf.Clamp01(result[key]);
        }

        return result;
    }

    public void ClearWeaponChances(Weapon weapon)
    {
        if (_weaponChances.ContainsKey(weapon))
            _weaponChances.Remove(weapon);
    }

    /// <summary>
    /// Получить список всех модификаторов для конкретного эффекта (глобальных и оружейных).
    /// </summary>
    public List<(ChanceSource source, float value)> GetChancesForEffect(OnHitEffectType type, Weapon weapon = null)
    {
        var result = new List<(ChanceSource, float)>();

        var global = _globalChances.Find(c => c.EffectType == type);
        if (global != null)
        {
            foreach (var kvp in global.GetAllModifiers())
                result.Add((kvp.Key, kvp.Value));
        }

        if (weapon != null && _weaponChances.TryGetValue(weapon, out var weaponList))
        {
            var local = weaponList.Find(c => c.EffectType == type);
            if (local != null)
            {
                foreach (var kvp in local.GetAllModifiers())
                    result.Add((kvp.Key, kvp.Value));
            }
        }

        return result;
    }

    /// <summary>
    /// Получить итоговый шанс (глобально + оружейные).
    /// </summary>
    public float GetTotalChanceForEffect(OnHitEffectType type, Weapon weapon = null)
    {
        float total = 0f;

        var global = _globalChances.Find(c => c.EffectType == type);
        if (global != null)
        {
            foreach (var kv in global.GetAllModifiers())
                total += kv.Value;
        }

        if (weapon != null && _weaponChances.TryGetValue(weapon, out var list))
        {
            var local = list.Find(c => c.EffectType == type);
            if (local != null)
            {
                foreach (var kv in local.GetAllModifiers())
                    total += kv.Value;
            }
        }

        return Mathf.Clamp01(total);
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
        Base,
        Weapon,
        Buff,
        Event,
        Perk,
        Other
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

        public IReadOnlyDictionary<ChanceSource, float> GetAllModifiers()
        {
            return _modifiers;
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
