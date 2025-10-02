using UnityEngine;
using Zenject;
using System.IO;

public class DataManager
{
    [Inject] private WeaponDatabaseSO _weaponDb;
    private PlayerData _playerData;

    public PlayerData PlayerData
    {
        get
        {
            if (_playerData == null)
                Load();
            return _playerData;
        }
    }

    public ICurrencyWriter CurrencyWriter => PlayerData;

    public void UpgradeWeapon(string weaponId, UpgradeType type)
    {
        var weapon = PlayerData.GetWeaponProgress(weaponId);
        weapon.Upgrade(type);
        Save();
    }

    public void Save()
    {
        string json = JsonUtility.ToJson(PlayerData, true);
        File.WriteAllText(GetPath(), json);
    }

    public void Load()
    {
        var path = GetPath();
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            _playerData = JsonUtility.FromJson<PlayerData>(json);
        }
        else
        {
            _playerData = new PlayerData();

            // создаём прогресс под все оружия из базы
            foreach (var w in _weaponDb.weapons)
            {
                _playerData.weapons.Add(new WeaponProgress
                {
                    weaponId = w.WeaponId,
                    damageLevel = 0,
                    fireRateLevel = 0,
                    healthLevel = 0
                });
            }
            Save();
        }
    }

    private string GetPath() =>
        Path.Combine(Application.persistentDataPath, "playerdata.json");
}
