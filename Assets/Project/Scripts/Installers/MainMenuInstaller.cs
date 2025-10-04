using UnityEngine;
using Zenject;

public class MainMenuInstaller : MonoInstaller
{
    [Header("UI References")]
    [SerializeField] private CurrencyFlyAnimator currencyFlyAnimator;
    [SerializeField] private UpgradeWeaponPanel upgradeWeaponPanel;
    [SerializeField] private WeaponsCarousel weaponsCarousel;

    public override void InstallBindings()
    {
        // ---------- UI Singletons ----------
        Container.Bind<CurrencyFlyAnimator>().FromInstance(currencyFlyAnimator).AsSingle();
        Container.Bind<UpgradeWeaponPanel>().FromInstance(upgradeWeaponPanel).AsSingle();
        Container.Bind<WeaponsCarousel>().FromInstance(weaponsCarousel).AsSingle();

        // ---------- Managers ----------
        Container.Bind<TimerManager>().AsSingle();

        // ---------- Post-Inject связывание EconomyService → CurrencyFlyAnimator ----------
        Container.BindInterfacesAndSelfTo<EconomyAnimatorLinker>().AsSingle()
            .WithArguments(currencyFlyAnimator);
    }
}

/// <summary>
/// После инициализации связывает EconomyService с CurrencyFlyAnimator
/// </summary>
public class EconomyAnimatorLinker : IInitializable
{
    private readonly EconomyService _economyService;
    private readonly CurrencyFlyAnimator _animator;

    public EconomyAnimatorLinker(EconomyService economyService, CurrencyFlyAnimator animator)
    {
        _economyService = economyService;
        _animator = animator;
    }

    public void Initialize()
    {
        _economyService.SetAnimator(_animator);
    }
}
