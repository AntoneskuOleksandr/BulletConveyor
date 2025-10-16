using UnityEngine;
using Zenject;

public class MainMenuInstaller : MonoInstaller
{
    [Header("UI References")]
    [SerializeField] private CurrencyFlyAnimator currencyFlyAnimator;
    [SerializeField] private UpgradeWeaponPanel upgradeWeaponPanel;
    [SerializeField] private WeaponsCarousel weaponsCarousel;
    [SerializeField] private MainMenuUIManager mainMenuUIManager;
    [SerializeField] private MainMenuController mainMenuController;
    [SerializeField] private LevelDisplay levelDisplay;

    public override void InstallBindings()
    {
        Container.BindInstance(currencyFlyAnimator).AsSingle();
        Container.BindInstance(upgradeWeaponPanel).AsSingle();
        Container.BindInstance(weaponsCarousel).AsSingle();
        Container.BindInstance(mainMenuUIManager).AsSingle();
        Container.BindInstance(mainMenuController).AsSingle();
        Container.BindInstance(levelDisplay).AsSingle();

        Container.BindInterfacesAndSelfTo<EconomyAnimatorLinker>().AsSingle()
            .WithArguments(currencyFlyAnimator);
    }
}

public class EconomyAnimatorLinker : IInitializable
{
    private readonly EconomyService _economy;
    private readonly CurrencyFlyAnimator _animator;

    public EconomyAnimatorLinker(EconomyService economy, CurrencyFlyAnimator animator)
    {
        _economy = economy;
        _animator = animator;
    }

    public void Initialize() => _economy.SetAnimator(_animator);
}
