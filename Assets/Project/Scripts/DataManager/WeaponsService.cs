using System;
using UnityEngine;
using Zenject;

public class WeaponsService
{
    [Inject] private DataManager _dataManager;
    [Inject] private EconomyService _economy;

    public event Action OnWeaponsChanged;

    public WeaponProgress GetWeaponProgress(int weaponId)
    {
        if (_dataManager.PlayerData.Weapons.GetProgress(weaponId) == null)
        {
            Debug.LogError($"No weapon progress found for weaponId {weaponId}");
            return null;
        }
        return _dataManager.PlayerData.Weapons.GetProgress(weaponId);
    }

    public bool UpgradeWeapon(WeaponSO weapon, UpgradeType type)
    {
        var progress = GetWeaponProgress(weapon.WeaponId);
        int level = GetLevel(progress, type);

        if (level >= weapon.upgradeLevels.Length)
            return false;

        int cost = weapon.upgradeLevels[level].cost;
        if (!_economy.SpendCurrency(cost))
            return false;

        progress.Upgrade(type);
        _dataManager.Save();

        OnWeaponsChanged?.Invoke();
        return true;
    }

    private int GetLevel(WeaponProgress progress, UpgradeType type)
    {
        return type switch
        {
            UpgradeType.Damage => progress.damageLevel,
            UpgradeType.FireRate => progress.fireRateLevel,
            UpgradeType.Health => progress.healthLevel,
            _ => 0
        };
    }
}
