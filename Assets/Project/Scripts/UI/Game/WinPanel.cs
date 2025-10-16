using Zenject;

public class WinPanel : ResultPanelBase
{
    [Inject] private LevelService _levelService;

    protected override void OnAfterReward()
    {
        _levelService.IncreaseLevel();
    }
}
