using System;
using UnityEngine;

public class EconomyService
{
    public event Action OnCurrencyChanged;

    private readonly DataManager _dataManager;
    private CurrencyFlyAnimator _animator;
    private PlayerData _playerData;

    public int Currency => _playerData.currency;
    public EconomyService(PlayerData playerData, DataManager dataManager)
    {
        _playerData = playerData;
        _dataManager = dataManager;

        Debug.Log($"[EconomyService] PlayerData: {_playerData}, DataManager: {_dataManager}");
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

        _dataManager.Save();
    }

    public void AddCurrencyDirect(int amount)
    {
        _playerData.currency += amount;
        OnCurrencyChanged?.Invoke();
    }

    public bool TrySpend(int amount)
    {
        if (_playerData.currency < amount) return false;
        _playerData.currency -= amount;
        OnCurrencyChanged?.Invoke();
        return true;
    }
}
