using UnityEngine;
using Zenject;

public class DataManager : IInitializable
{
    private readonly DataRepository _repository;
    private readonly WeaponDatabaseSO _db;
    private PlayerData _playerData;

    public PlayerData PlayerData
    {
        get
        {
            if (_playerData == null)
                Initialize();

            return _playerData;
        }
        private set => _playerData = value;
    }
    public DataManager(DataRepository repository, WeaponDatabaseSO db)
    {
        _repository = repository;
        _db = db;
    }

    public void Initialize()
    {
        PlayerData = _repository.Load();
        if (PlayerData == null)
        {
            PlayerData = new PlayerData();
            _repository.Save(PlayerData);
            Debug.Log("[DataManager] Новый PlayerData создан");
        }
        else
        {
            Debug.Log("[DataManager] PlayerData успешно загружен");
        }

        // Убедимся, что оружие синхронизировано с базой
        EnsureAllWeaponsExist();
    }

    public void Save()
    {
        _repository.Save(PlayerData);
        //Debug.Log("[DataManager] PlayerData сохранён");
    }

    private void EnsureAllWeaponsExist()
    {
        foreach (var weapon in _db.weapons)
        {
            if (PlayerData.weapons.FindIndex(w => w.id == weapon.WeaponId) < 0)
                PlayerData.weapons.Add(new WeaponEntry { id = weapon.WeaponId });
        }
    }
}
