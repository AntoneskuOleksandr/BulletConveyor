using System.Collections.Generic;

public class StrategyResolver
{
    private readonly Dictionary<ShootStrategyType, IShootStrategy> _shootStrategies;
    private readonly Dictionary<AimStrategyType, IAimStrategy> _aimStrategies;
    private readonly Dictionary<TargetStrategyType, ITargetStrategy> _targetStrategies;

    public StrategyResolver(
        BasicShootStrategy basicShoot,
        ShotgunShootStrategy shotgunShoot,
        BasicAimStrategy basicAim,
        BallisticAimStrategy ballisticAim,
        NearestTargetStrategy nearestTarget)
    {
        _shootStrategies = new Dictionary<ShootStrategyType, IShootStrategy>
        {
            { ShootStrategyType.Basic, basicShoot },
            {ShootStrategyType.Shotgun, shotgunShoot },
        };

        _aimStrategies = new Dictionary<AimStrategyType, IAimStrategy>
        {
            { AimStrategyType.Basic, basicAim },
            {AimStrategyType.Ballistic, ballisticAim },
        };

        _targetStrategies = new Dictionary<TargetStrategyType, ITargetStrategy>
        {
            { TargetStrategyType.Nearest, nearestTarget },
        };
    }

    public IShootStrategy ResolveShoot(ShootStrategyType type) => _shootStrategies[type];
    public IAimStrategy ResolveAim(AimStrategyType type) => _aimStrategies[type];
    public ITargetStrategy ResolveTarget(TargetStrategyType type) => _targetStrategies[type];

    public List<IAimStrategy> ResolveAims(IEnumerable<AimStrategyType> types)
    {
        var list = new List<IAimStrategy>();
        foreach (var t in types)
        {
            if (_aimStrategies.TryGetValue(t, out var s))
                list.Add(s);
            else
                throw new KeyNotFoundException($"AimStrategy not registered: {t}");
        }
        return list;
    }

    public List<IAimStrategy> ResolveAims(AimStrategyType type) => ResolveAims(new[] { type });
}
