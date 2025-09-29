using UnityEngine;
using Zenject;

public class MainMenuInstaller : MonoInstaller
{
    [SerializeField] private CurrencyFlyAnimator currencyFlyAnimator;
    public override void InstallBindings()
    {
        Container.Bind<CurrencyFlyAnimator>().FromInstance(currencyFlyAnimator).AsSingle();
        Container.Bind<EconomyManager>().AsSingle();
        Container.Bind<TimerManager>().AsSingle();
    }
}
