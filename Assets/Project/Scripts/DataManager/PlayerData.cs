using System;
using System.Collections.Generic;

[Serializable]
public class PlayerData : IReadOnlyPlayerData
{
    public int currency;
    public List<WeaponEntry> weapons = new();
    public List<int> selectedWeaponIds = new();

    public int Currency => currency;
    public IReadOnlyList<WeaponEntry> Weapons => weapons;
    public IReadOnlyList<int> SelectedWeaponIds => selectedWeaponIds;
}

[Serializable]
public struct WeaponEntry
{
    public int id;
    public int dmgLvl;
    public int fireRateLvl;
    public int healthLvl;
}
