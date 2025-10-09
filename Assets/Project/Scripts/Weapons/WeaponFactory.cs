using System;
using UnityEngine;
using Zenject;

public class WeaponFactory
{
    private readonly DiContainer _container;

    public WeaponFactory(DiContainer container)
    {
        _container = container;
    }

    public Weapon AddWeaponComponent(GameObject go, WeaponSO so)
    {
        Weapon weapon = so.WeaponType switch
        {
            WeaponType.MachineGun => _container.InstantiateComponent<MachineGun>(go),
            _ => throw new ArgumentOutOfRangeException($"Unsupported WeaponType: {so.WeaponType}")
        };

        weapon.InitializeFromSO(so);
        return weapon;
    }
}
