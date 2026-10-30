using Zenject;

public class TutorialProgressService
{
    public bool IsTutorialCompleted => _playerData.isTutorialCompleted;
    public bool ShouldPlayTutorial => !IsTutorialCompleted || replayRequested;
    private bool replayRequested;

    private PlayerData _playerData;
    private readonly DataManager _dataManager;

    public TutorialProgressService(PlayerData playerData, DataManager dataManager)
    {
        _playerData = playerData;
        _dataManager = dataManager;
    }

    public void CompleteTutorial()
    {
        replayRequested = false;
        _playerData.isTutorialCompleted = true;
        _dataManager.Save();
    }

    public void RequestReplay() => replayRequested = true;
}
