using System;

public class PlayerProgressService
{
    public event Action<int> OnLevelChanged;

    private readonly DataManager _dataManager;
    private readonly PlayerData _playerData;

    public PlayerProgressService(PlayerData playerData, DataManager dataManager)
    {
        _playerData = playerData;
        _dataManager = dataManager;
    }

    public int GetCurrentLevel() => _playerData.PlayerLevel;

    public void SetSelectedLevel(int index)
    {
        _playerData.selectedLevelIndex = index;
        _dataManager.Save();
    }

    public int GetSelectedLevel()
    {
        return _playerData.selectedLevelIndex;
    }

    public bool SetPlayerLevel(int level)
    {
        if (level < 1) return false;
        if (_playerData.playerLevel == level) return false;

        _playerData.playerLevel = level;
        _dataManager.Save();
        OnLevelChanged?.Invoke(_playerData.playerLevel);
        return true;
    }

    public void IncreasePlayerLevel(int by = 1)
    {
        if (by <= 0) return;
        _playerData.playerLevel += by;
        _dataManager.Save();
        OnLevelChanged?.Invoke(_playerData.playerLevel);
    }

    public bool CanAccessLevel(int requiredLevel) => _playerData.PlayerLevel >= requiredLevel;
}
