using System;
using UnityEngine;
using Zenject;

public class EconomyManager
{
    public event Action OnCurrencyChanged;

    [Inject] private DataManager _dataManager;
    [Inject] private ICurrencyWriter _writer;

    private CurrencyFlyAnimator _animator;

    private PlayerData PlayerData => _dataManager.PlayerData;
    public int Currency => PlayerData.Currency;

    public void SetAnimator(CurrencyFlyAnimator animator)
    {
        _animator = animator;
    }

    public void AddCurrency(int amount, bool useAnimation = false, RectTransform fromUI = null)
    {
        if (amount <= 0) return;

        if (useAnimation && fromUI != null && _animator != null)
        {
            _animator.AnimateCurrencyGain(amount, fromUI);
        }
        else
            AddCurrencyDirect(amount);
    }

    public void AddCurrencyDirect(int amount)
    {
        if (amount <= 0) return;

        _writer.SetCurrency(PlayerData.Currency + amount);

        OnCurrencyChanged?.Invoke();
        _dataManager.Save();
    }

    public bool SpendCurrency(int amount)
    {
        if (PlayerData.Currency < amount)
            return false;

        _writer.SetCurrency(PlayerData.Currency - amount);

        OnCurrencyChanged?.Invoke();
        _dataManager.Save();
        return true;
    }
}
