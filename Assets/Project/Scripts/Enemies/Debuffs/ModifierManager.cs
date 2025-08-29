using System.Collections.Generic;

public class ModifierManager
{
    private Dictionary<ModifierType, List<float>> _modifiers = new();

    // Добавить новый модификатор
    public void AddModifier(ModifierType type, float value)
    {
        if (!_modifiers.ContainsKey(type))
            _modifiers[type] = new List<float>();

        _modifiers[type].Add(value);
    }

    // Удалить модификатор (например, при истечении дебафа)
    public void RemoveModifier(ModifierType type, float value)
    {
        if (_modifiers.ContainsKey(type))
        {
            _modifiers[type].Remove(value);
            if (_modifiers[type].Count == 0)
                _modifiers.Remove(type);
        }
    }

    // Получить итоговое значение модификатора
    public float GetTotalModifier(ModifierType type)
    {
        if (!_modifiers.ContainsKey(type))
            return 0f;

        float total = 0f;
        foreach (var v in _modifiers[type])
            total += v; // можно здесь сделать и умножение, если нужен множитель

        return total;
    }

    public enum ModifierType
    {
        FireDamage,
        IceDamage,
        MoveSpeed,
        Scale
    }

}
