using System.Collections.Generic;
using UnityEngine;

public class EffectChanceManager
{
    private List<ProjectileEffectChance> _chances = new List<ProjectileEffectChance>();

    public void RegisterEffectChance(OnHitEffectType type, float value, ChanceSource source = ChanceSource.Base)
    {
        var existing = _chances.Find(c => c.EffectType == type);
        if (existing == null)
        {
            existing = new ProjectileEffectChance { EffectType = type };
            _chances.Add(existing);
        }
        existing.AddModifier(source, value);
    }

    public void OverrideEffectChance(OnHitEffectType type, float value, ChanceSource source)
    {
        var existing = _chances.Find(c => c.EffectType == type);
        if (existing == null)
        {
            existing = new ProjectileEffectChance { EffectType = type };
            _chances.Add(existing);
        }
        existing.SetModifier(source, value);
    }

    public void Clear() => _chances.Clear();

    public List<OnHitEffectType> RollEffects()
    {
        List<OnHitEffectType> results = new List<OnHitEffectType>();
        foreach (var c in _chances)
        {
            if (Random.value < c.TotalChance)
                results.Add(c.EffectType);
        }
        return results;
    }

    public enum ChanceSource
    {
        Base,       // Базовый шанс (например, от выбора абилки)
        Weapon,     // Бонус от оружия
        Buff,       // Бонус от баффа
        Event,      // Бонус от события
        Perk,       // Бонус от постоянного перка
        Other       // На будущее, для любых других источников
    }

    [System.Serializable]
    public class ProjectileEffectChance
    {
        public OnHitEffectType EffectType;

        private Dictionary<ChanceSource, float> _modifiers = new Dictionary<ChanceSource, float>();

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
