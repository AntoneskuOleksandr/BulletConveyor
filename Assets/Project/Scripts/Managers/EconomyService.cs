using System;
using UnityEngine;
using Zenject;

public class EconomyService
{
    public event Action OnCurrencyChanged;

    [Inject] private DataManager _dataManager;

    private CurrencyFlyAnimator _animator;

    private PlayerData Data => _dataManager.Data;
    public int Currency => Data.currency;


    /// <summary>
    /// Привязывает UI-аниматор валюты (CurrencyFlyAnimator) к сервису.
    /// </summary>
    public void SetAnimator(CurrencyFlyAnimator animator)
    {
        _animator = animator;
    }

    /// <summary>
    /// Добавляет валюту (с анимацией, если нужно).
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
    /// Добавляет валюту напрямую без анимации.
    /// </summary>
    public void AddCurrencyDirect(int amount)
    {
        if (amount <= 0) return;

        Data.currency += amount;
        OnCurrencyChanged?.Invoke();
    }

    /// <summary>
    /// Пробует потратить валюту. Возвращает true, если успешно.
    /// </summary>
    public bool TrySpend(int amount)
    {
        if (amount <= 0) return true; // 0 не тратим
        if (Data.currency < amount) return false;

        Data.currency -= amount;
        OnCurrencyChanged?.Invoke();
        return true;
    }
}
