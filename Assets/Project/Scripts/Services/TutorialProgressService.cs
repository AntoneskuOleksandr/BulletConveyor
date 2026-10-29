using Zenject;

public class TutorialProgressService
{
    public bool IsTutorialCompleted => _playerData.isTutorialCompleted;

    private PlayerData _playerData;
    private readonly DataManager _dataManager;

    public TutorialProgressService(PlayerData playerData, DataManager dataManager)
    {
        _playerData = playerData;
        _dataManager = dataManager;
    }

    public void CompleteTutorial()
    {
        _playerData.isTutorialCompleted = true;
        _dataManager.Save();
    }
}
