using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public class WeaponsService
{
    public event Action OnWeaponsChanged;
    public event Action OnSelectionChanged;

    private readonly DataManager _dataManager;
    private readonly WeaponDatabaseSO _db;
    private readonly EconomyService _economyService;
    private readonly PlayerProgressService _playerProgressService;
    private PlayerData _playerData;

    private readonly int _maxSlots = 3;

    [Inject]
    public WeaponsService(PlayerData playerData, DataManager dataManager, WeaponDatabaseSO db, EconomyService economy, PlayerProgressService playerProgressService)
    {
        _playerData = playerData;
        _dataManager = dataManager;
        _db = db;
        _economyService = economy;
        _playerProgressService = playerProgressService;
    }

    public WeaponProgressRO GetProgressRO(int id)
    {
        var entry = _playerData.weapons.Find(w => w.id == id);
        return entry.id == 0 && id != 0
            ? new WeaponProgressRO(id, 0, 0, 0)
            : new WeaponProgressRO(entry.id, entry.dmgLvl, entry.fireRateLvl, entry.healthLvl);
    }

    public bool TryUpgrade(WeaponSO weapon, UpgradeType type)
    {
        int idx = _playerData.weapons.FindIndex(w => w.id == weapon.WeaponId);
        if (idx < 0)
        {
            _playerData.weapons.Add(new WeaponEntry { id = weapon.WeaponId });
            idx = _playerData.weapons.Count - 1;
        }

        var entry = _playerData.weapons[idx];
        int level = type switch
        {
            UpgradeType.Damage => entry.dmgLvl,
            UpgradeType.FireRate => entry.fireRateLvl,
            UpgradeType.Health => entry.healthLvl,
            _ => 0
        };

        if (level >= weapon.UpgradeLevels.Length) return false;
        if (!_economyService.TrySpend(CurrencyType.Gears, weapon.UpgradeLevels[level].Cost)) return false;

        switch (type)
        {
            case UpgradeType.Damage: entry.dmgLvl++; break;
            case UpgradeType.FireRate: entry.fireRateLvl++; break;
            case UpgradeType.Health: entry.healthLvl++; break;
        }

        _playerData.weapons[idx] = entry;
        _dataManager.Save();
        OnWeaponsChanged?.Invoke();
        return true;
    }

    public List<WeaponSO> GetSelectedWeapons()
    {
        var result = new List<WeaponSO>();
        foreach (var id in _playerData.selectedWeaponIds)
        {
            var so = _db.weapons.Find(w => w.WeaponId == id);
            if (so != null)
                result.Add(so);
        }
        return result;
    }

    public List<WeaponSO> GetAvailableWeapons()
    {
        var lvl = _playerProgressService.GetCurrentLevel();
        return _db.weapons.Where(w => w.RequiredLevel <= lvl).ToList();
    }

    public bool IsSelected(int id) => _playerData.selectedWeaponIds.Contains(id);
    public bool IsAvailable(int id)
    {
        var so = _db.weapons.Find(w => w.WeaponId == id);
        return so != null && so.RequiredLevel <= _playerProgressService.GetCurrentLevel();
    }

    public int GetSelectedCount() => _playerData.selectedWeaponIds.Count;

    public int GetMaxSlots() => _maxSlots;

    public bool SelectWeapon(int id, int maxSlots = -1)
    {
        var so = _db.weapons.Find(w => w.WeaponId == id);
        if (so == null)
            return false;

        if (so.RequiredLevel > _playerProgressService.GetCurrentLevel())
        {
            Debug.LogError($"[WeaponsService] Weapon {so.name} (ID: {id}) is not available at current level {_playerProgressService.GetCurrentLevel()}.");
            return false;
        }

        int limit = maxSlots <= 0 ? _maxSlots : maxSlots;
        if (_playerData.selectedWeaponIds.Contains(id) || _playerData.selectedWeaponIds.Count >= limit)
            return false;

        _playerData.selectedWeaponIds.Add(id);
        _dataManager.Save();
        OnSelectionChanged?.Invoke();
        return true;
    }

    public bool DeselectWeapon(int id)
    {
        if (!_playerData.selectedWeaponIds.Remove(id)) return false;
        _dataManager.Save();
        OnSelectionChanged?.Invoke();
        return true;
    }
}
