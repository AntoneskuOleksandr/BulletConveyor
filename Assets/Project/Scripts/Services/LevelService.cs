using System;

public class LevelService
{
    public event Action<int> OnLevelChanged;

    private readonly DataManager _dataManager;
    private readonly PlayerData _playerData;

    public LevelService(PlayerData playerData, DataManager dataManager)
    {
        _playerData = playerData;
        _dataManager = dataManager;
    }

    public int GetCurrentLevel() => _playerData.CurrentLevel;

    public bool SetLevel(int level)
    {
        if (level < 1) return false;
        if (_playerData.currentLevel == level) return false;

        _playerData.currentLevel = level;
        _dataManager.Save();
        OnLevelChanged?.Invoke(_playerData.currentLevel);
        return true;
    }

    public void IncreaseLevel(int by = 1)
    {
        if (by <= 0) return;
        _playerData.currentLevel += by;
        _dataManager.Save();
        OnLevelChanged?.Invoke(_playerData.currentLevel);
    }

    public bool CanAccessLevel(int requiredLevel) => _playerData.CurrentLevel >= requiredLevel;
}
