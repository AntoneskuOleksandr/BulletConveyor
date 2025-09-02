using System;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public Action OnMoneyChanged;
    public float Money { get; private set; }

    public void AddMoney(float amount)
    {
        Money += amount;
        OnMoneyChanged?.Invoke();
    }

    public bool SpendMoney(float amount)
    {
        if (Money < amount)
            return false;

        Money -= amount;
        return true;
    }
}
