using System;

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

        EnsureAllCurrenciesExist(_playerData);

        _repository.Save(_playerData);
        return _playerData;
    }

    private void EnsureAllCurrenciesExist(PlayerData data)
    {
        foreach (CurrencyType type in Enum.GetValues(typeof(CurrencyType)))
        {
            if (!data.currencies.Exists(c => c.type == type))
            {
                data.currencies.Add(new CurrencyEntry { type = type, amount = 0 });
            }
        }
    }

    public void Save() => _repository.Save(_playerData);
}
