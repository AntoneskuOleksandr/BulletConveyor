public class DataManager
{
    private readonly DataRepository _repository;
    private PlayerData _playerData;

    public DataManager(DataRepository repository)
    {
        _repository = repository;
    }

    public PlayerData LoadOrCreate()
    {
        _playerData = _repository.Load() ?? new PlayerData();
        _repository.Save(_playerData);
        return _playerData;
    }

    public void Save() => _repository.Save(_playerData);
}
