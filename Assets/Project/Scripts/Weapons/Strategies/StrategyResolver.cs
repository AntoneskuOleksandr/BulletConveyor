using Zenject;

public class StrategyResolver
{
    private readonly IShootStrategy _basicShoot;

    private readonly IAimStrategy _basicAim;

    private readonly ITargetStrategy _nearestTarget;

    public StrategyResolver(
        IShootStrategy basicShoot,
        IAimStrategy basicAim,
        ITargetStrategy nearestTarget
        )
    {
        _basicShoot = basicShoot;
        _basicAim = basicAim;
        _nearestTarget = nearestTarget;
    }

    public IShootStrategy ResolveShoot(ShootStrategyType type) => type switch
    {
        ShootStrategyType.Basic => _basicShoot,
        _ => _basicShoot
    };

    public IAimStrategy ResolveAim(AimStrategyType type) => type switch
    {
        AimStrategyType.Basic => _basicAim,
        _ => _basicAim
    };

    public ITargetStrategy ResolveTarget(TargetStrategyType type) => type switch
    {
        TargetStrategyType.Nearest => _nearestTarget,
        _ => _nearestTarget
    };
}
