public class SaveManager
{
    private readonly DataRepository _repo;
    private readonly PlayerData _data;

    public SaveManager(DataRepository repo, PlayerData data)
    {
        _repo = repo;
        _data = data;
    }

    public void Register(EconomyService economy, WeaponsService weapons)
    {
        economy.OnCurrencyChanged += Save;
        weapons.OnWeaponsChanged += Save;
        weapons.OnSelectionChanged += Save;
    }

    private void Save()
    {
        _repo.Save(_data);
    }
}
