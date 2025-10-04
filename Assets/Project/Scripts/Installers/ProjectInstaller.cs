using UnityEngine;
using Zenject;

public class ProjectInstaller : MonoInstaller
{
    [SerializeField] private WeaponDatabaseSO weaponDatabase;

    public override void InstallBindings()
    {
        // ---------- Core Scriptable Objects ----------
        Container.Bind<WeaponDatabaseSO>().FromInstance(weaponDatabase).AsSingle();

        // ---------- Data Layer ----------
        Container.Bind<DataRepository>().AsSingle();
        Container.Bind<DataManager>().AsSingle();

        // ---------- Services ----------
        Container.Bind<EconomyService>().AsSingle();
        Container.Bind<WeaponsService>().AsSingle();
        Container.Bind<SaveManager>().AsSingle();

        // ---------- Инициализация данных ----------
        Container.QueueForInject(this); // чтобы OnInstallerLoaded вызывался после всех Bind

        // ---------- Запускаем начальную инициализацию ----------
        Container.BindInterfacesAndSelfTo<ProjectInitializer>().AsSingle();
    }
}

/// <summary>
/// Выполняет стартовую инициализацию данных и сервисов
/// </summary>
public class ProjectInitializer : IInitializable
{
    private readonly DataManager _dataManager;
    private readonly WeaponsService _weaponsService;
    private readonly EconomyService _economyService;
    private readonly SaveManager _saveManager;

    public ProjectInitializer(
        DataManager dataManager,
        WeaponsService weaponsService,
        EconomyService economyService,
        SaveManager saveManager)
    {
        _dataManager = dataManager;
        _weaponsService = weaponsService;
        _economyService = economyService;
        _saveManager = saveManager;
    }

    public void Initialize()
    {
        // Загружаем данные игрока
        _dataManager.Initialize();

        // Создаём оружие под базу (если первый запуск)
        _weaponsService.EnsureAllWeaponsExist();

        // Подписываем SaveManager на изменения
        _saveManager.Register(_economyService, _weaponsService);
    }
}
