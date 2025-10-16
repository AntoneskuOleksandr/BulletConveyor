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

        Container.Bind<LevelService>().AsSingle()
            .WithArguments(playerData);

        Container.Bind<EconomyService>()
            .AsSingle()
            .WithArguments(playerData);

        Container.Bind<WeaponsService>()
            .AsSingle()
            .WithArguments(playerData);

        Container.BindInterfacesAndSelfTo<SaveManager>().AsSingle();


        Container.Bind<TimerManager>().AsSingle();
    }
}
