using UnityEngine;
using Zenject;

public class MainMenuInstaller : MonoInstaller
{
    [SerializeField] private CurrencyFlyAnimator currencyFlyAnimator;
    [SerializeField] private UpgradeWeaponPanel upgradeWeaponPanel;
    [SerializeField] private WeaponsCarousel weaponsCarousel;

    [Inject] private EconomyService _economyService;

    public override void InstallBindings()
    {
        Container.Bind<CurrencyFlyAnimator>().FromInstance(currencyFlyAnimator).AsSingle();
        Container.Bind<UpgradeWeaponPanel>().FromInstance(upgradeWeaponPanel).AsSingle();
        Container.Bind<WeaponsCarousel>().FromInstance(weaponsCarousel).AsSingle();
        Container.Bind<WeaponsService>().AsSingle();
        Container.Bind<TimerManager>().AsSingle();

        _economyService.SetAnimator(currencyFlyAnimator);
    }
}
