using UnityEngine;
using Zenject;

public class WeaponFactory
{
    private readonly DiContainer _container;
    private readonly WeaponsService _weaponsService;
    private readonly StrategyResolver _strategyResolver;
    private readonly WeaponModifierResolver _modifierResolver;

    public WeaponFactory(DiContainer container, WeaponsService weaponsService, StrategyResolver strategyResolver, WeaponModifierResolver modifierResolver)
    {
        _container = container;
        _weaponsService = weaponsService;
        _strategyResolver = strategyResolver;
        _modifierResolver = modifierResolver;
    }

    public Weapon AddWeaponComponent(GameObject go, WeaponSO so)
    {
        // 1. Создаём компонент оружия
        Weapon weapon = _container.InstantiateComponent<Weapon>(go);

        // 2. Назначаем стратегии пуль
        IBulletMoveStrategy move = so.BulletMoveType switch
        {
            BulletMoveType.Straight => new StraightMoveStrategy(),
            BulletMoveType.Arc => new ArcMoveStrategy(),
            _ => new StraightMoveStrategy()
        };

        IBulletHitStrategy hit = so.BulletHitType switch
        {
            BulletHitType.Direct => new DirectHitStrategy(),
            BulletHitType.AoE => new AoEHitStrategy(so.GetParameter(WeaponParameterType.AoERadius, 5f)),
            _ => new DirectHitStrategy()
        };

        // 3. Назначаем стратегии поведения
        var shoot = _strategyResolver.ResolveShoot(so.ShootStrategy);
        var aim = _strategyResolver.ResolveAims(so.AimStrategies);
        var target = _strategyResolver.ResolveTarget(so.TargetStrategy);
        var modifiers = _modifierResolver.ResolveModifiers(so.Modifiers);

        weapon.SetWeaponStrategy(
            aim, so.AimStrategies,
            shoot, so.ShootStrategy,
            target, so.TargetStrategy
        );

        weapon.SetBulletStrategy(
            move, so.BulletMoveType,
            hit, so.BulletHitType
        );

        weapon.SetWeaponModifiers(modifiers, so.Modifiers);

        var progress = _weaponsService.GetProgressRO(so.WeaponId);
        weapon.InitializeFromSO(so, progress);

        return weapon;
    }

}
