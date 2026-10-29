using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Zenject;

public class MainMenuUIManager : MonoBehaviour
{
    [SerializeField] private TMP_Text moneyText;
    [SerializeField] private Button startGameButton;
    [SerializeField] private string gameSceneName = "GameScene";
    [SerializeField] private GameObject lockPanel;

    [Inject] private EconomyService _economyService;
    [Inject] private WeaponsService _weaponsService;
    [Inject] private PlayerProgressService _playerProgressService;
    [Inject] private LevelDisplay _levelDisplay;
    [Inject] private TutorialProgressService _tutorialProgressService;

    private void OnEnable()
    {
        _economyService.OnCurrencyChanged += OnCurrencyChanged;
        _weaponsService.OnSelectionChanged += UpdateStartButtonState;
        _levelDisplay.OnLevelChanged += UpdateStartButtonState;

        startGameButton.onClick.AddListener(StartGame);

        UpdateStartButtonState();
        UpdateMoneyDisplay();
    }

    private void OnDisable()
    {
        _economyService.OnCurrencyChanged -= OnCurrencyChanged;
        _weaponsService.OnSelectionChanged -= UpdateStartButtonState;
        _levelDisplay.OnLevelChanged -= UpdateStartButtonState;
        startGameButton.onClick.RemoveListener(StartGame);
    }

    public void SetLockPanelState(bool show)
    {
        lockPanel.SetActive(show);
    }

    private void UpdateStartButtonState()
    {
        if (!_levelDisplay.TryGetCurrentLevel(out int levelIndex))
        {
            startGameButton.interactable = false;
            return;
        }

        bool hasSelectedWeapons = !_tutorialProgressService.IsTutorialCompleted || _weaponsService.GetSelectedCount() > 0;
        bool isLevelAvailable = _playerProgressService.CanAccessLevel(levelIndex);

        startGameButton.interactable = hasSelectedWeapons && isLevelAvailable;
    }

    private void StartGame()
    {
        if (!startGameButton.interactable || !_levelDisplay.TryGetCurrentLevel(out int levelIndex))
            return;

        _playerProgressService.SetSelectedLevel(levelIndex);
        SceneManager.LoadScene(gameSceneName);
    }

    private void UpdateMoneyDisplay()
    {
        float gears = _economyService.GetCurrency(CurrencyType.Gears);
        moneyText.text = gears.ToString();
    }

    private void OnCurrencyChanged(CurrencyType type) => UpdateMoneyDisplay();
}
