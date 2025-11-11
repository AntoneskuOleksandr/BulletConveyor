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
        IProjectileMoveStrategy move = so.ProjectileMoveType switch
        {
            ProjectileMoveType.Straight => new StraightMoveStrategy(),
            ProjectileMoveType.Arc => new ArcMoveStrategy(),
            ProjectileMoveType.BeamInstant => new BeamInstantMoveStrategy(),
            ProjectileMoveType.BeamContinuous => new BeamContinuousMoveStrategy(),
            _ => new StraightMoveStrategy()
        };

        IProjectileHitStrategy hit = so.ProjectileHitType switch
        {
            ProjectileHitType.Direct => new DirectHitStrategy(),
            ProjectileHitType.AoE => new AoEHitStrategy(),
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

        weapon.SetProjectileStrategy(
            move, so.ProjectileMoveType,
            hit, so.ProjectileHitType
        );

        weapon.SetWeaponModifiers(modifiers, so.Modifiers);

        var progress = _weaponsService.GetProgressRO(so.WeaponId);
        weapon.InitializeFromSO(so, progress);

        return weapon;
    }

}
