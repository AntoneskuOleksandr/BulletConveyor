using System;
using UnityEngine;

public class EconomyService
{
    public event Action<CurrencyType> OnCurrencyChanged;

    private readonly DataManager _dataManager;
    private readonly PlayerData _playerData;
    private CurrencyFlyAnimator _animator;

    public EconomyService(PlayerData playerData, DataManager dataManager)
    {
        _playerData = playerData;
        _dataManager = dataManager;
    }

    public void SetAnimator(CurrencyFlyAnimator animator)
    {
        _animator = animator;
    }

    public float GetCurrency(CurrencyType type)
    {
        var entry = _playerData.currencies.Find(c => c.type == type);
        return entry.type == type ? entry.amount : 0f;
    }

    public void AddCurrency(CurrencyType type, float amount, bool animated = false, RectTransform from = null)
    {
        if (amount <= 0) return;

        if (animated && from != null && _animator != null)
            _animator.AnimateCurrencyGain(amount, from, a => ModifyCurrency(type, a));
        else
            ModifyCurrency(type, amount);
    }

    public bool TrySpend(CurrencyType type, float amount)
    {
        if (amount <= 0) return false;

        float current = GetCurrency(type);
        if (current < amount)
        {
            Debug.LogWarning($"[EconomyService] Not enough {type}. Required: {amount}, current: {current}");
            return false;
        }

        ModifyCurrency(type, -amount);
        return true;
    }

    private void ModifyCurrency(CurrencyType type, float delta)
    {
        int index = _playerData.currencies.FindIndex(c => c.type == type);

        if (index < 0)
        {
            Debug.LogError($"[EconomyService] Currency of type {type} not found!");
            return;
        }

        var entry = _playerData.currencies[index];
        entry.amount += delta;
        if (entry.amount < 0) entry.amount = 0;
        _playerData.currencies[index] = entry;

        OnCurrencyChanged?.Invoke(type);
        _dataManager.Save();
    }
}
