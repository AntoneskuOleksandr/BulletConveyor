using System;
using System.Collections.Generic;

[Serializable]
public class PlayerData
{
    public int currency;
    public List<WeaponEntry> weapons = new();         // Прогресс оружия
    public List<int> selectedWeaponIds = new();       // Выбранные пушки
}

[Serializable]
public struct WeaponEntry
{
    public int id;
    public int dmgLvl;
    public int fireRateLvl;
    public int healthLvl;
}
