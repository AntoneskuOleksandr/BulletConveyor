using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class WeaponsService
{
    public event Action OnWeaponsChanged;
    public event Action OnSelectionChanged;

    private readonly PlayerData _data;
    private readonly WeaponDatabaseSO _db;
    private readonly EconomyService _economy;

    [Inject]
    public WeaponsService(PlayerData data, WeaponDatabaseSO db, EconomyService economy)
    {
        _data = data;
        _db = db;
        _economy = economy;
    }

    // ----------- Чтение прогресса -----------
    public WeaponProgressRO GetProgressRO(int id)
    {
        var e = _data.weapons.Find(w => w.id == id);
        return e.id == 0 && id != 0
            ? new WeaponProgressRO(id, 0, 0, 0)
            : new WeaponProgressRO(e.id, e.dmgLvl, e.fireRateLvl, e.healthLvl);
    }

    // ----------- Апгрейд -----------
    public bool TryUpgrade(WeaponSO weapon, UpgradeType type)
    {
        int idx = _data.weapons.FindIndex(w => w.id == weapon.WeaponId);
        if (idx < 0)
        {
            _data.weapons.Add(new WeaponEntry { id = weapon.WeaponId });
            idx = _data.weapons.Count - 1;
        }

        var entry = _data.weapons[idx];
        int level = type switch
        {
            UpgradeType.Damage => entry.dmgLvl,
            UpgradeType.FireRate => entry.fireRateLvl,
            UpgradeType.Health => entry.healthLvl,
            _ => 0
        };

        if (weapon.upgradeLevels == null || level >= weapon.upgradeLevels.Length)
            return false;

        int cost = weapon.upgradeLevels[level].cost;
        if (!_economy.TrySpend(cost))
            return false;

        switch (type)
        {
            case UpgradeType.Damage: entry.dmgLvl++; break;
            case UpgradeType.FireRate: entry.fireRateLvl++; break;
            case UpgradeType.Health: entry.healthLvl++; break;
        }

        _data.weapons[idx] = entry;
        OnWeaponsChanged?.Invoke();
        return true;
    }

    // ----------- Выбор оружия -----------
    public IReadOnlyList<int> GetSelectedWeaponIds() => _data.selectedWeaponIds.AsReadOnly();

    public List<WeaponSO> GetSelectedWeapons()
    {
        var res = new List<WeaponSO>();
        foreach (var id in _data.selectedWeaponIds)
        {
            var so = _db.weapons.Find(w => w.WeaponId == id);
            if (so != null)
                res.Add(so);
        }
        return res;
    }

    public bool IsSelected(int id) => _data.selectedWeaponIds.Contains(id);

    public bool SelectWeapon(int id, int maxSlots = 3)
    {
        if (_data.selectedWeaponIds.Contains(id)) return false;
        if (_data.selectedWeaponIds.Count >= maxSlots) return false;
        _data.selectedWeaponIds.Add(id);
        OnSelectionChanged?.Invoke();
        return true;
    }

    public bool DeselectWeapon(int id)
    {
        if (!_data.selectedWeaponIds.Remove(id)) return false;
        OnSelectionChanged?.Invoke();
        return true;
    }

    public void ClearSelection()
    {
        if (_data.selectedWeaponIds.Count == 0) return;
        _data.selectedWeaponIds.Clear();
        OnSelectionChanged?.Invoke();
    }

    // ----------- Инициализация при старте -----------
    public void EnsureAllWeaponsExist()
    {
        foreach (var w in _db.weapons)
        {
            if (_data.weapons.FindIndex(e => e.id == w.WeaponId) < 0)
                _data.weapons.Add(new WeaponEntry { id = w.WeaponId });
        }
    }
}
