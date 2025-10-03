using System;
using UnityEngine;

public interface ICurrencyWriter
{
    void SetCurrency(int newValue);
}

[Serializable]
public class PlayerData : ICurrencyWriter
{
    public int Currency;
    public WeaponsData Weapons;

    public PlayerData()
    {
        Weapons = new WeaponsData();
    }

    void ICurrencyWriter.SetCurrency(int newValue)
    {
        Currency = Mathf.Max(0, newValue);
        Debug.Log($"[PlayerData] Currency set to {Currency}");
    }
}

