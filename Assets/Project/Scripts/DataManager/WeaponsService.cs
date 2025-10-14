using System;
using System.Collections.Generic;
using Zenject;

public class WeaponsService
{
    public event Action OnWeaponsChanged;
    public event Action OnSelectionChanged;

    private readonly DataManager _dataManager;
    private readonly WeaponDatabaseSO _db;
    private readonly EconomyService _economy;

    private PlayerData Data => _dataManager.PlayerData;

    private readonly int _maxSlots = 3;

    [Inject]
    public WeaponsService(DataManager dataManager, WeaponDatabaseSO db, EconomyService economy)
    {
        _dataManager = dataManager;
        _db = db;
        _economy = economy;
    }

    public WeaponProgressRO GetProgressRO(int id)
    {
        var entry = Data.weapons.Find(w => w.id == id);
        return entry.id == 0 && id != 0
            ? new WeaponProgressRO(id, 0, 0, 0)
            : new WeaponProgressRO(entry.id, entry.dmgLvl, entry.fireRateLvl, entry.healthLvl);
    }

    public bool TryUpgrade(WeaponSO weapon, UpgradeType type)
    {
        int idx = Data.weapons.FindIndex(w => w.id == weapon.WeaponId);
        if (idx < 0)
        {
            Data.weapons.Add(new WeaponEntry { id = weapon.WeaponId });
            idx = Data.weapons.Count - 1;
        }

        var entry = Data.weapons[idx];
        int level = type switch
        {
            UpgradeType.Damage => entry.dmgLvl,
            UpgradeType.FireRate => entry.fireRateLvl,
            UpgradeType.Health => entry.healthLvl,
            _ => 0
        };

        if (level >= weapon.upgradeLevels.Length) return false;
        if (!_economy.TrySpend(weapon.upgradeLevels[level].cost)) return false;

        switch (type)
        {
            case UpgradeType.Damage: entry.dmgLvl++; break;
            case UpgradeType.FireRate: entry.fireRateLvl++; break;
            case UpgradeType.Health: entry.healthLvl++; break;
        }

        Data.weapons[idx] = entry;
        _dataManager.Save();
        OnWeaponsChanged?.Invoke();
        return true;
    }

    public List<WeaponSO> GetSelectedWeapons()
    {
        var result = new List<WeaponSO>();
        foreach (var id in Data.selectedWeaponIds)
        {
            var so = _db.weapons.Find(w => w.WeaponId == id);
            if (so != null)
                result.Add(so);
        }
        return result;
    }

    public bool IsSelected(int id) => Data.selectedWeaponIds.Contains(id);

    public int GetSelectedCount() => Data.selectedWeaponIds.Count;

    public int GetMaxSlots() => _maxSlots;

    public bool SelectWeapon(int id, int maxSlots = -1)
    {
        int limit = maxSlots <= 0 ? _maxSlots : maxSlots;
        if (Data.selectedWeaponIds.Contains(id) || Data.selectedWeaponIds.Count >= limit)
            return false;

        Data.selectedWeaponIds.Add(id);
        _dataManager.Save();
        OnSelectionChanged?.Invoke();
        return true;
    }

    public bool DeselectWeapon(int id)
    {
        if (!Data.selectedWeaponIds.Remove(id)) return false;
        _dataManager.Save();
        OnSelectionChanged?.Invoke();
        return true;
    }
}
