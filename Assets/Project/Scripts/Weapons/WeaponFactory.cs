using UnityEngine;
using Zenject;

public class WeaponFactory
{
    private readonly DiContainer _container;
    private readonly WeaponsService _weaponsService;
    private readonly StrategyResolver _strategyResolver;

    public WeaponFactory(DiContainer container, WeaponsService weaponsService, StrategyResolver resolver)
    {
        _container = container;
        _weaponsService = weaponsService;
        _strategyResolver = resolver;
    }

    public Weapon AddWeaponComponent(GameObject go, WeaponSO so)
    {
        Weapon weapon = _container.InstantiateComponent<MachineGun>(go);

        var progress = _weaponsService.GetProgressRO(so.WeaponId);
        weapon.InitializeFromSO(so, progress);

        var shoot = _strategyResolver.ResolveShoot(so.shootStrategy);
        var aim = _strategyResolver.ResolveAim(so.aimStrategy);
        var target = _strategyResolver.ResolveTarget(so.targetStrategy);

        weapon.SetStrategies(
            shoot, so.shootStrategy,
            aim, so.aimStrategy,
            target, so.targetStrategy
        );

        return weapon;
    }

}
