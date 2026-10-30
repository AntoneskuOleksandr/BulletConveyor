using Zenject;

public class StrategyInstaller : MonoInstaller
{
    public override void InstallBindings()
    {

        Container.Bind<BasicShootStrategy>().AsSingle();
        Container.Bind<MultiBarrelShootStrategy>().AsSingle();

        Container.Bind<BasicAimStrategy>().AsSingle();
        Container.Bind<BallisticAimStrategy>().AsSingle();

        Container.Bind<NearestTargetStrategy>().AsSingle();

        Container.Bind<StraightMoveStrategy>().AsSingle();
        Container.Bind<ArcMoveStrategy>().AsSingle();

        Container.Bind<DirectHitStrategy>().AsSingle();
        Container.Bind<AoEHitStrategy>().AsSingle();

        Container.Bind<StrategyResolver>().AsSingle();
    }
}
