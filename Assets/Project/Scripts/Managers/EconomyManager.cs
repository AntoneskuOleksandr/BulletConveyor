using System;
using UnityEngine;
using Zenject;

public class EconomyManager
{
    public event Action OnMoneyChanged;
    public int Money { get; private set; }

    [Inject] private CurrencyFlyAnimator _animator;

    public void AddMoney(int amount, RectTransform fromUI = null, bool useAnimation = true)
    {
        if (useAnimation && fromUI != null)
        {
            _animator.AnimateCurrencyGain(amount, fromUI);
        }
        else
        {
            Money += amount;
            OnMoneyChanged?.Invoke();
        }
    }

    public void AddMoneyDirect(int amount)
    {
        Money += amount;
        OnMoneyChanged?.Invoke();
    }

    public bool SpendMoney(int amount)
    {
        if (Money < amount) return false;
        Money -= amount;
        OnMoneyChanged?.Invoke();
        return true;
    }
}
