using Zenject;

public class WinPanel : ResultPanelBase
{
    [Inject] private PlayerProgressService _playerProgressService;
    [Inject] private TutorialProgressService _tutorialProgressService;

    protected override void OnAfterReward()
    {
        var game = UnityEngine.Object.FindAnyObjectByType<GameManager>();
        if (game != null && game.IsTrainingReplay) return;
        _playerProgressService.IncreasePlayerLevel();

        if (!_tutorialProgressService.IsTutorialCompleted)
        {
            _tutorialProgressService.CompleteTutorial();
        }
    }
}
