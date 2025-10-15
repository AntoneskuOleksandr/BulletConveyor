using System.Linq;
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

    [Inject] private EconomyService _economyService;
    [Inject] private WeaponsService _weaponsService;

    private void OnEnable()
    {
        _economyService.OnCurrencyChanged += (currencyType) =>
        {
            UpdateMoneyDisplay();
        };
        _weaponsService.OnSelectionChanged += () =>
        {
            startGameButton.interactable = _weaponsService.GetSelectedCount() > 0;
        };

        startGameButton.interactable = _weaponsService.GetSelectedCount() > 0;
        startGameButton.onClick.AddListener(StartGame);

        UpdateMoneyDisplay();
    }

    private void OnDestroy()
    {
        _economyService.OnCurrencyChanged -= (currencyType) => { UpdateMoneyDisplay(); };
        startGameButton.onClick.RemoveListener(StartGame);
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
