using Zenject;

public class StrategyInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        // Shoot strategies
        Container.Bind<BasicShootStrategy>().AsSingle();
        Container.Bind<ShotgunShootStrategy>().AsSingle();
        Container.Bind<PeashooterShootStrategy>().AsSingle();

        // Aim strategies
        Container.Bind<BasicAimStrategy>().AsSingle();
        Container.Bind<BallisticAimStrategy>().AsSingle();

        // Targeting strategies
        Container.Bind<NearestTargetStrategy>().AsSingle();

        // Bullet movement strategies
        Container.Bind<StraightMoveStrategy>().AsSingle();
        Container.Bind<ArcMoveStrategy>().AsSingle();

        // Bullet hit strategies
        Container.Bind<DirectHitStrategy>().AsSingle();
        Container.Bind<AoEHitStrategy>().AsSingle();

        //Resolver
        Container.Bind<StrategyResolver>().AsSingle();
    }
}
