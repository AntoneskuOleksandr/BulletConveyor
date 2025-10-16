using Zenject;

public class WinPanel : ResultPanelBase
{
    [Inject] private PlayerProgressService _playerProgressService;
    [Inject] private TutorialProgressService _tutorialProgressService;

    protected override void OnAfterReward()
    {
        _playerProgressService.IncreasePlayerLevel();

        if (!_tutorialProgressService.IsTutorialCompleted)
        {
            _tutorialProgressService.CompleteTutorial();
        }
    }
}
