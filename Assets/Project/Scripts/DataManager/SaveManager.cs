using Zenject;
using UnityEngine;

public class SaveManager : IInitializable
{
    private readonly DataRepository _repo;
    private readonly DataManager _data;
    private readonly EconomyService _economy;
    private readonly WeaponsService _weapons;

    public SaveManager(DataRepository repo, DataManager data, EconomyService economy, WeaponsService weapons)
    {
        Debug.Log("[SaveManager] Constructor called.");
        _repo = repo;
        _data = data;
        _economy = economy;
        _weapons = weapons;
    }

    public void Initialize()
    {
        Debug.Log("[SaveManager] Initializing and subscribing to events...");
        _economy.OnCurrencyChanged += Save;
        _weapons.OnWeaponsChanged += Save;
        _weapons.OnSelectionChanged += Save;
    }

    private void Save()
    {
        Debug.Log("[SaveManager] Saving PlayerData...");
        _repo.Save(_data.PlayerData);
    }
}
