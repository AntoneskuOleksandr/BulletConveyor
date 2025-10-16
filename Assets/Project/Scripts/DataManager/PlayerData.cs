using System;
using System.Collections.Generic;

[Serializable]
public class PlayerData : IReadOnlyPlayerData
{
    public List<CurrencyEntry> currencies = new();
    public List<WeaponEntry> weapons = new();
    public List<int> selectedWeaponIds = new();
    public int playerLevel = 0;
    public int selectedLevelIndex = 0;
    public bool isTutorialCompleted = false;

    public IReadOnlyList<CurrencyEntry> Currencies => currencies;
    public IReadOnlyList<WeaponEntry> Weapons => weapons;
    public IReadOnlyList<int> SelectedWeaponIds => selectedWeaponIds;
    public int PlayerLevel => playerLevel;
    public int SelectedLevelIndex => selectedLevelIndex;
    public bool IsTutorialCompleted => isTutorialCompleted;
}

[Serializable]
public struct CurrencyEntry
{
    public CurrencyType type;
    public float amount;
}

[Serializable]
public struct WeaponEntry
{
    public int id;
    public int dmgLvl;
    public int fireRateLvl;
    public int healthLvl;
}
