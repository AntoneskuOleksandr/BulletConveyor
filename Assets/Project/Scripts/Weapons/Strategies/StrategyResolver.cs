using System.Collections.Generic;

public class StrategyResolver
{
    private readonly Dictionary<ShootStrategyType, IShootStrategy> _shootStrategies;
    private readonly Dictionary<AimStrategyType, IAimStrategy> _aimStrategies;
    private readonly Dictionary<TargetStrategyType, ITargetStrategy> _targetStrategies;

    public StrategyResolver(
        BasicShootStrategy basicShoot,
        BasicAimStrategy basicAim,
        NearestTargetStrategy nearestTarget)
    {
        _shootStrategies = new Dictionary<ShootStrategyType, IShootStrategy>
        {
            { ShootStrategyType.Basic, basicShoot },
        };

        _aimStrategies = new Dictionary<AimStrategyType, IAimStrategy>
        {
            { AimStrategyType.Basic, basicAim },
        };

        _targetStrategies = new Dictionary<TargetStrategyType, ITargetStrategy>
        {
            { TargetStrategyType.Nearest, nearestTarget },
        };
    }

    public IShootStrategy ResolveShoot(ShootStrategyType type) => _shootStrategies[type];
    public IAimStrategy ResolveAim(AimStrategyType type) => _aimStrategies[type];
    public ITargetStrategy ResolveTarget(TargetStrategyType type) => _targetStrategies[type];
}
