using Zenject;

public class WinPanel : ResultPanelBase
{
    [Inject] private PlayerProgressService _playerProgressService;

    protected override void OnAfterReward()
    {
        _playerProgressService.IncreasePlayerLevel();
    }
}
