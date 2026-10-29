using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
public class UIManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private AbilityPanel abilityPanel;
    [SerializeField] private PauseMenuPanel pauseMenuPanel;
    [SerializeField] private WinPanel winPanel;
    [SerializeField] private LosePanel losePanel;

    [Header("Text")]
    [SerializeField] private TMP_Text playerCoins;
    [SerializeField] private TMP_Text buyBlockText;

    [Header("Buttons")]
    [SerializeField] private Button buyBlockButton;
    [SerializeField] private Button buyBlockByAdButton;
    [SerializeField] private Button speedUpGameButton;
    [SerializeField] private Button pauseGameButton;

    [SerializeField] private float timeSpeedUpValue = 3f;

    [Header("Progress Bar")]
    [SerializeField] private ProgressBar progressBar;

    [Header("Tutorial")]
    [SerializeField] private TutorialManager tutorialManager;
    [Inject] private TutorialProgressService _tutorialProgressService;

    [Inject] private BlockManager _blockManager;
    [Inject] private PlayerRuntimeManager _playerManager;
    [Inject] private WaveManager _waveManager;

    private void OnEnable()
    {
        if (pauseGameButton != null && pauseMenuPanel != null)
        {
            pauseGameButton.gameObject.SetActive(true);
            pauseGameButton.onClick.AddListener(pauseMenuPanel.ShowPanel);
        }

        if (!_tutorialProgressService.IsTutorialCompleted)
        {
            if (buyBlockButton != null)
            {
                buyBlockButton.onClick.AddListener(OnBuyButtonClicked);

                buyBlockByAdButton.gameObject.SetActive(false);
                speedUpGameButton.gameObject.SetActive(false);
                if (_playerManager != null)
                {
                    _playerManager.OnCurrencyChanged += UpdateUI;
                }
            }
        }
        else
        {
            if (buyBlockButton != null)
            {
                buyBlockButton.onClick.AddListener(OnBuyButtonClicked);
            }
            if (buyBlockByAdButton != null)
            {
                buyBlockByAdButton.onClick.AddListener(OnBuyBlockByAdButtonClicked);
            }
            if (speedUpGameButton != null)
            {
                speedUpGameButton.onClick.AddListener(OnSpeedUpButtonClicked);
            }
            if (_playerManager != null)
            {
                _playerManager.OnCurrencyChanged += UpdateUI;
            }
            if (_waveManager != null && abilityPanel != null)
            {
                _waveManager.OnTimerBetweenWavesStarted += OnTimerBetweenWavesStarted;
            }
        }
    }

    private void OnDisable()
    {
        buyBlockButton.onClick.RemoveListener(OnBuyButtonClicked);
        buyBlockByAdButton.onClick.RemoveListener(OnBuyBlockByAdButtonClicked);
        speedUpGameButton.onClick.RemoveListener(OnSpeedUpButtonClicked);
        pauseGameButton.onClick.RemoveListener(pauseMenuPanel.ShowPanel);
        _playerManager.OnCurrencyChanged -= UpdateUI;
        _waveManager.OnTimerBetweenWavesStarted -= OnTimerBetweenWavesStarted;
    }

    private void OnTimerBetweenWavesStarted(int waveIndex, float timeBetweenWaves)
    {
        if (waveIndex % 2 == 0) abilityPanel.ShowPanel();
    }

    private void Start()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        playerCoins.text = _playerManager.GetCurrency(CurrencyType.Coins).ToString();
        buyBlockText.text = _blockManager.BlockPrice.ToString();
        buyBlockButton.interactable = _playerManager.GetCurrency(CurrencyType.Coins) >= _blockManager.BlockPrice;
    }

    private void OnBuyButtonClicked()
    {
        if (_playerManager.GetCurrency(CurrencyType.Coins) >= _blockManager.BlockPrice && _blockManager.CanCreateBlock())
        {
            _playerManager.TrySpend(CurrencyType.Coins, _blockManager.BlockPrice);
            _blockManager.CreateBlock();
        }

        UpdateUI();
    }

    private void OnBuyBlockByAdButtonClicked()
    {
        if (_blockManager != null)
        {
            _blockManager.CreateBlock(true);
        }
        else
        {
            Debug.LogError("BlockManager is not initialized. Cannot create block.");
        }

        UpdateUI();
    }

    private void OnSpeedUpButtonClicked()
    {
        float newTimeSpeedValue = (Time.timeScale == timeSpeedUpValue) ? 1f : timeSpeedUpValue;

        speedUpGameButton.GetComponentInChildren<TMP_Text>().text = newTimeSpeedValue.ToString();
        Time.timeScale = newTimeSpeedValue;
    }

    public void ShowWinPanel()
    {
        tutorialManager.HideTutorial();
        if (winPanel != null)
        {
            winPanel.ShowPanel();
        }
        else
        {
            Debug.LogError("WinPanel is not assigned in the inspector.");
        }
    }

    public void ShowLosePanel()
    {
        tutorialManager.HideTutorial();
        if (losePanel != null)
        {
            losePanel.ShowPanel();
        }
        else
        {
            Debug.LogError("LosePanel is not assigned in the inspector.");
        }
    }
}
