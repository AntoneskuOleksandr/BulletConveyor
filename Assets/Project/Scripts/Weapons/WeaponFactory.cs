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
        // 1. Создаём компонент оружия
        Weapon weapon = _container.InstantiateComponent<Weapon>(go);

        // 2. Назначаем стратегии пуль
        IBulletMoveStrategy move = so.bulletMoveType switch
        {
            BulletMoveType.Straight => new StraightMoveStrategy(),
            BulletMoveType.Arc => new ArcMoveStrategy(),
            _ => new StraightMoveStrategy()
        };

        IBulletHitStrategy hit = so.bulletHitType switch
        {
            BulletHitType.Direct => new DirectHitStrategy(),
            BulletHitType.AoE => new AoEHitStrategy(so.GetParameter(WeaponParameterType.AoERadius, 5f)),
            _ => new DirectHitStrategy()
        };

        // 3. Назначаем стратегии поведения
        var shoot = _strategyResolver.ResolveShoot(so.shootStrategy);
        var aim = _strategyResolver.ResolveAims(so.aimStrategies);
        var target = _strategyResolver.ResolveTarget(so.targetStrategy);

        weapon.SetWeaponStrategy(
            shoot, so.shootStrategy,
            aim, so.aimStrategies,
            target, so.targetStrategy
        );

        weapon.SetBulletStrategy(
            move, so.bulletMoveType,
            hit, so.bulletHitType
        );

        var progress = _weaponsService.GetProgressRO(so.WeaponId);
        weapon.InitializeFromSO(so, progress);

        return weapon;
    }

}
