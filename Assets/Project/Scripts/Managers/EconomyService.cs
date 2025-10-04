using System;
using UnityEngine;

public class EconomyService
{
    public event Action OnCurrencyChanged;

    private readonly DataManager _dataManager;
    private CurrencyFlyAnimator _animator;

    private PlayerData Data => _dataManager.PlayerData;
    public int Currency => Data.currency;

    public EconomyService(DataManager dataManager)
    {
        _dataManager = dataManager;
    }

    public void SetAnimator(CurrencyFlyAnimator animator)
    {
        _animator = animator;
    }

    public void AddCurrency(int amount, bool animated = false, RectTransform from = null)
    {
        if (amount <= 0) return;

        if (animated && from != null && _animator != null)
            _animator.AnimateCurrencyGain(amount, from, AddCurrencyDirect);
        else
            AddCurrencyDirect(amount);
    }

    public void AddCurrencyDirect(int amount)
    {
        Data.currency += amount;
        OnCurrencyChanged?.Invoke();
    }

    public bool TrySpend(int amount)
    {
        if (Data.currency < amount) return false;
        Data.currency -= amount;
        OnCurrencyChanged?.Invoke();
        return true;
    }
}
