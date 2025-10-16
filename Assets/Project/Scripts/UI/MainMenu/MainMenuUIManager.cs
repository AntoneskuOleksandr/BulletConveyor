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
    [Inject] private LevelService _levelService;
    [Inject] private LevelDisplay _levelDisplay;

    private void OnEnable()
    {
        _economyService.OnCurrencyChanged += (currencyType) =>
        {
            UpdateMoneyDisplay();
        };
        _weaponsService.OnSelectionChanged += () =>
        {
            UpdateStartButtonState();
        };
        _levelDisplay.OnLevelChanged += () =>
        {
            UpdateStartButtonState();
        };

        startGameButton.onClick.AddListener(StartGame);

        UpdateStartButtonState();
        UpdateMoneyDisplay();
    }

    private void OnDestroy()
    {
        _economyService.OnCurrencyChanged -= (currencyType) => { UpdateMoneyDisplay(); };
        startGameButton.onClick.RemoveListener(StartGame);
    }

    public void SetLockPanelState(bool show)
    {
        lockPanel.SetActive(show);
    }

    private void UpdateStartButtonState()
    {
        bool hasSelectedWeapons = _weaponsService.GetSelectedCount() > 0;
        bool isLevelAvailable = _levelService.CanAccessLevel(_levelDisplay.GetCurrentLevel());

        startGameButton.interactable = hasSelectedWeapons && isLevelAvailable;
    }

    private void StartGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    private void UpdateMoneyDisplay()
    {
        float gears = _economyService.GetCurrency(CurrencyType.Gears);
        moneyText.text = gears.ToString();
    }
}
