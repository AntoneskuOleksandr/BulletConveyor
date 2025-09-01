using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ModifierManager : MonoBehaviour
{
    public enum ModifierType
    {
        FireDamageTaken,
        IceDamageTaken,
        Slow,
        Stun,
    }

    private Dictionary<ModifierType, float> _modifiers = new Dictionary<ModifierType, float>();

    private Enemy _enemy; // ссылка на врага
    private float _baseSpeed;

    private void Awake()
    {
        _enemy = GetComponent<Enemy>();
        if (_enemy != null && _enemy.GetComponent<NavMeshAgent>() != null)
            _baseSpeed = _enemy.GetComponent<NavMeshAgent>().speed;
    }

    public void AddModifier(ModifierType type, float value)
    {
        _modifiers[type] = value;
    }

    public void RemoveModifier(ModifierType type)
    {
        if (_modifiers.ContainsKey(type))
            _modifiers.Remove(type);
    }

    public float ApplyModifiers(ModifierType type, float baseValue)
    {
        if (_modifiers.TryGetValue(type, out float multiplier))
            return baseValue * multiplier;

        return baseValue;
    }

    public List<string> GetActiveModifiers()
    {
        List<string> list = new List<string>();
        foreach (var kv in _modifiers)
            list.Add($"{kv.Key}: {kv.Value}");
        return list;
    }
}
