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
        Debug.Log("[MainMenuInstaller] Installing main menu bindings...");
        Container.BindInstance(currencyFlyAnimator).AsSingle();
        Container.BindInstance(upgradeWeaponPanel).AsSingle();
        Container.BindInstance(weaponsCarousel).AsSingle();

        Container.BindInterfacesAndSelfTo<EconomyAnimatorLinker>().AsSingle()
            .WithArguments(currencyFlyAnimator);

        Debug.Log("[MainMenuInstaller] Main menu bindings installed.");
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
