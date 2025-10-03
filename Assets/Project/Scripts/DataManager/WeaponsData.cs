using System;
using System.Collections.Generic;

[Serializable]
public class WeaponsData
{
    public List<WeaponProgress> weapons = new List<WeaponProgress>();

    public WeaponProgress GetProgress(int id)
    {
        return weapons.Find(w => w.weaponId == id);
    }

    public void AddWeapon(int id)
    {
        if (weapons.Exists(w => w.weaponId == id))
            return;

        weapons.Add(new WeaponProgress
        {
            weaponId = id,
            damageLevel = 0,
            fireRateLevel = 0,
            healthLevel = 0
        });
    }
}

[Serializable]
public class WeaponProgress
{
    public int weaponId;
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
