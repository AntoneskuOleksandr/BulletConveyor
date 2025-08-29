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
        if (type == ModifierType.Slow)
            UpdateSpeed();
    }

    public void RemoveModifier(ModifierType type)
    {
        if (_modifiers.ContainsKey(type))
            _modifiers.Remove(type);

        if (type == ModifierType.Slow)
            UpdateSpeed();
    }

    public float ApplyModifiers(ModifierType type, float baseValue)
    {
        if (_modifiers.TryGetValue(type, out float multiplier))
            return baseValue * multiplier;

        return baseValue;
    }

    private void UpdateSpeed()
    {
        if (_enemy == null) return;

        float slowMultiplier = 1f;
        if (_modifiers.TryGetValue(ModifierType.Slow, out float multiplier))
            slowMultiplier = multiplier;

        var agent = _enemy.GetComponent<NavMeshAgent>();
        if (agent != null)
            agent.speed = _baseSpeed * slowMultiplier;

        var animator = _enemy.GetComponent<Animator>();
        if (animator != null)
            animator.SetFloat("MoveSpeed", agent.speed / 2f); // для анимации
    }

    public List<string> GetActiveModifiers()
    {
        List<string> list = new List<string>();
        foreach (var kv in _modifiers)
            list.Add($"{kv.Key}: {kv.Value}");
        return list;
    }
}
