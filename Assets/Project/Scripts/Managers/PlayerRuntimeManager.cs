using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRuntimeManager : MonoBehaviour
{
    public event Action OnCurrencyChanged;
    private List<CurrencyEntry> _currencies = new();

    private void OnEnable()
    {
        _currencies.Add(new CurrencyEntry
        {
            type = CurrencyType.Coins,
            amount = 0f
        });
        _currencies.Add(new CurrencyEntry
        {
            type = CurrencyType.Gears,
            amount = 0f
        });
    }

    public float GetCurrency(CurrencyType type)
    {
        var entry = _currencies.Find(c => c.type == type);
        return entry.type == type ? entry.amount : 0f;
    }

    public void AddCurrency(CurrencyType type, float amount)
    {
        if (amount <= 0) return;
        ModifyCurrency(type, amount);
    }

    public bool TrySpend(CurrencyType type, float amount)
    {
        if (amount <= 0) return false;

        float current = GetCurrency(type);

        if (current < amount) return false;

        ModifyCurrency(type, -amount);
        return true;
    }

    private void ModifyCurrency(CurrencyType type, float delta)
    {
        int index = _currencies.FindIndex(c => c.type == type);

        if (index < 0)
        {
            Debug.LogError($"[PlayerRuntimeManager] Currency of type {type} not found!");
            return;
        }

        var entry = _currencies[index];
        entry.amount += delta;
        if (entry.amount < 0) entry.amount = 0;
        _currencies[index] = entry;

        OnCurrencyChanged?.Invoke();
    }
}
