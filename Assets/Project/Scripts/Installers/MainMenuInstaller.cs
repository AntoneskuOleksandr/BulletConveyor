using Zenject;

public class MainMenuInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<EconomyManager>().AsSingle();
        Container.Bind<TimerManager>().AsSingle();
    }
}
