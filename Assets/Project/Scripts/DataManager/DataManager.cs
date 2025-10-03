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

    public void UpgradeWeapon(int weaponId, UpgradeType type)
    {
        var weapon = PlayerData.Weapons.GetProgress(weaponId);
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
        string path = GetPath();
        if (File.Exists(path))
        {
            Debug.Log($"Loading player data from {path}");
            string json = File.ReadAllText(path);
            _playerData = JsonUtility.FromJson<PlayerData>(json);
        }
        else
        {
            Debug.Log("No save file found, creating new player data");
            _playerData = new PlayerData();

            // создаём прогресс под все оружия из базы
            foreach (WeaponSO weapon in _weaponDb.weapons)
            {
                _playerData.Weapons.AddWeapon(weapon.WeaponId);
            }

            Save();
        }
    }

    private string GetPath() => Path.Combine(Application.persistentDataPath, "playerdata.json");
}
