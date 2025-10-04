using UnityEngine;
using Zenject;

public class ProjectInstaller : MonoInstaller
{
    [SerializeField] private WeaponDatabaseSO weaponDatabase;

    public override void InstallBindings()
    {
        Debug.Log("[ProjectInstaller] Installing project-wide bindings...");
        // --- Core data ---
        Container.BindInstance(weaponDatabase).AsSingle();
        Container.Bind<DataRepository>().AsSingle();
        Container.BindInterfacesAndSelfTo<DataManager>().AsSingle().NonLazy();

        // --- Services ---
        Container.Bind<EconomyService>().AsSingle();
        Container.Bind<WeaponsService>().AsSingle(); 
        Container.BindInterfacesAndSelfTo<SaveManager>().AsSingle();


        // --- Optional UI-independent systems ---
        Container.Bind<TimerManager>().AsSingle();

        Debug.Log("[ProjectInstaller] All bindings installed.");
    }
}
