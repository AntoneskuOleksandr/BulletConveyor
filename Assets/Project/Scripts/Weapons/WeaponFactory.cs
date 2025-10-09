using UnityEngine;
using Zenject;

public class WeaponFactory
{
    private readonly DiContainer _container;
    private readonly WeaponsService _weaponsService;

    public WeaponFactory(DiContainer container, WeaponsService weaponsService)
    {
        _container = container;
        _weaponsService = weaponsService;
    }

    public Weapon AddWeaponComponent(GameObject go, WeaponSO so)
    {
        Weapon weapon = so.WeaponType switch
        {
            WeaponType.MachineGun => _container.InstantiateComponent<MachineGun>(go),
            _ => _container.InstantiateComponent<MachineGun>(go) // fallback
        };

        var progress = _weaponsService.GetProgressRO(so.WeaponId);
        weapon.InitializeFromSO(so, progress);
        return weapon;
    }
}
