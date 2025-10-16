using Zenject;

public class TutorialProgressService
{
    public bool IsTutorialCompleted => _playerData.isTutorialCompleted;

    private WeaponsService _weaponsService;
    private PlayerData _playerData;

    public TutorialProgressService(PlayerData playerData, WeaponsService weaponsService)
    {
        _playerData = playerData;
        _weaponsService = weaponsService;
    }

    public void CompleteTutorial()
    {
        _playerData.isTutorialCompleted = true;
        _weaponsService.DeselectWeapon(0);
    }
}
