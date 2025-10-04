using Zenject;

public class DataManager
{
    private readonly DataRepository _repo;
    private readonly WeaponDatabaseSO _weaponDb;

    public PlayerData Data { get; private set; }
    public PlayerStateView ReadOnly { get; private set; }

    [Inject]
    public DataManager(DataRepository repo, WeaponDatabaseSO weaponDb)
    {
        _repo = repo;
        _weaponDb = weaponDb;
    }

    public void Initialize()
    {
        Data = _repo.Load();
        ReadOnly = new PlayerStateView(Data);
    }
}
