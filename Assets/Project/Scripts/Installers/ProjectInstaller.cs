using UnityEngine;
using Zenject;

public class ProjectInstaller : MonoInstaller
{
    [SerializeField] private WeaponDatabaseSO weaponDatabase;

    public override void InstallBindings()
    {
        Container.BindInstance(weaponDatabase).AsSingle();

        DataRepository dataRepository = new DataRepository();
        var dataManager = new DataManager(dataRepository);
        var playerData = dataManager.LoadOrCreate();

        Container.BindInstance(dataRepository).AsSingle();
        Container.BindInstance(dataManager).AsSingle();
        Container.Bind<IReadOnlyPlayerData>().FromInstance(playerData).AsSingle();

        Container.Bind<EconomyService>()
            .AsSingle()
            .WithArguments(playerData, dataManager);

        Container.Bind<WeaponsService>()
            .AsSingle()
            .WithArguments(playerData, dataManager, weaponDatabase);

        Container.BindInterfacesAndSelfTo<SaveManager>().AsSingle();


        Container.Bind<TimerManager>().AsSingle();
    }
}
