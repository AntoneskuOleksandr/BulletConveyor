using System;
using System.Collections.Generic;
using UnityEngine;

public interface ICurrencyWriter
{
    void SetCurrency(int newValue);
}

[Serializable]
public class PlayerData : ICurrencyWriter
{
    public int Currency { get; private set; }

    // доступен только через интерфейс
    void ICurrencyWriter.SetCurrency(int newValue)
    {
        Currency = Mathf.Max(0, newValue);
        Debug.Log($"[PlayerData] Currency set to {Currency}");
    }

    public string selectedWeaponId;
    public List<WeaponProgress> weapons = new List<WeaponProgress>();

    public WeaponProgress GetWeaponProgress(string id)
    {
        return weapons.Find(w => w.weaponId == id);
    }
}

[Serializable]
public class WeaponProgress
{
    public string weaponId;
    public int damageLevel;
    public int fireRateLevel;
    public int healthLevel;

    public void Upgrade(UpgradeType type)
    {
        switch (type)
        {
            case UpgradeType.Damage: damageLevel++; break;
            case UpgradeType.FireRate: fireRateLevel++; break;
            case UpgradeType.Health: healthLevel++; break;
        }
    }
}
