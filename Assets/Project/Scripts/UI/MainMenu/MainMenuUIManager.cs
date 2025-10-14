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

    [Inject] private EconomyService _economy;

    private void OnEnable()
    {
        _economy.OnCurrencyChanged += UpdateMoneyDisplay;
        startGameButton.onClick.AddListener(StartGame);

        UpdateMoneyDisplay();
    }

    private void OnDestroy()
    {
        _economy.OnCurrencyChanged -= UpdateMoneyDisplay;
        startGameButton.onClick.RemoveListener(StartGame);
    }

    private void StartGame()
    {
        Debug.Log("Start Game button clicked!");
        SceneManager.LoadScene(gameSceneName);
    }

    private void UpdateMoneyDisplay()
    {
        moneyText.text = _economy.Currency.ToString();
    }
}
