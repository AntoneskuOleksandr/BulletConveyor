using System;

public class EconomyManager
{
    public event Action OnMoneyChanged;
    public int Money { get; private set; }

    public void AddMoney(int amount)
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
