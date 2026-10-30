using UnityEngine;
using Zenject;

public class ProjectInstaller : MonoInstaller
{
    [SerializeField] private WeaponDatabaseSO weaponDatabase;
    [SerializeField] private bool useDebugPlayerData;
    [SerializeField] private PlayerDataEditorSO debugPlayerData;

    public override void InstallBindings()
    {
        Container.BindInstance(weaponDatabase).AsSingle();

        DataRepository dataRepository = new DataRepository();
        var dataManager = new DataManager(dataRepository);

        PlayerData playerData;

#if UNITY_EDITOR
        if (useDebugPlayerData && debugPlayerData != null)
        {

            dataRepository = new DataRepository(System.IO.Path.Combine(
                Application.persistentDataPath, "debug-playerdata.json"));
            dataManager = new DataManager(dataRepository);
            playerData = dataManager.LoadOrCreate(debugPlayerData.GetPlayerData());
        }
        else
#endif
            playerData = dataManager.LoadOrCreate();

        Container.BindInstance(dataRepository).AsSingle();
        Container.BindInstance(dataManager).AsSingle();
        Container.Bind<IReadOnlyPlayerData>().FromInstance(playerData).AsSingle();

        Container.Bind<PlayerProgressService>().AsSingle()
            .WithArguments(playerData);

        Container.Bind<EconomyService>()
            .AsSingle()
            .WithArguments(playerData);

        Container.Bind<WeaponsService>()
            .AsSingle()
            .WithArguments(playerData);

        Container.Bind<TutorialProgressService>()
            .AsSingle()
            .WithArguments(playerData);

        Container.Bind<TimerManager>().AsSingle();
    }

    private void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
    }
}
