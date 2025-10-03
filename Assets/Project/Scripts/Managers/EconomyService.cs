using System;
using UnityEngine;
using Zenject;

public class EconomyService
{
    public event Action OnCurrencyChanged;

    [Inject] private DataManager _dataManager;
    [Inject] private ICurrencyWriter _writer;

    private CurrencyFlyAnimator _animator;

    private PlayerData PlayerData => _dataManager.PlayerData;
    public int Currency => PlayerData.Currency;

    /// <summary>
    /// Связываем UI-аниматор валюты с сервисом
    /// </summary>
    public void SetAnimator(CurrencyFlyAnimator animator)
    {
        _animator = animator;
    }

    /// <summary>
    /// Добавляем валюту с анимацией (если нужно)
    /// </summary>
    public void AddCurrency(int amount, bool useAnimation = false, RectTransform fromUI = null)
    {
        if (amount <= 0) return;

        if (useAnimation && fromUI != null && _animator != null)
        {
            _animator.AnimateCurrencyGain(amount, fromUI, (reward) => AddCurrencyDirect(reward));
        }
        else
        {
            AddCurrencyDirect(amount);
        }
    }


    /// <summary>
    /// Добавляем валюту напрямую без анимации
    /// </summary>
    public void AddCurrencyDirect(int amount)
    {
        if (amount <= 0) return;

        _writer.SetCurrency(PlayerData.Currency + amount);

        _dataManager.Save();
        OnCurrencyChanged?.Invoke();
    }

    /// <summary>
    /// Тратим валюту. Вернёт true, если успешно.
    /// </summary>
    public bool SpendCurrency(int amount)
    {
        if (PlayerData.Currency < amount)
            return false;

        _writer.SetCurrency(PlayerData.Currency - amount);

        _dataManager.Save();
        OnCurrencyChanged?.Invoke();
        return true;
    }
}
