using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class UIManager : MonoBehaviour
{
    [SerializeField] private AbilityPanel abilityPanel;
    [SerializeField] private PauseMenuPanel pauseMenuPanel;
    [SerializeField] private WinPanel winPanel;
    [SerializeField] private LosePanel losePanel;
    [SerializeField] private TMP_Text playerCoins;
    [SerializeField] private TMP_Text buyBlockText;
    [SerializeField] private Button buyBlockButton;
    [SerializeField] private Button buyBlockByAdButton;
    [SerializeField] private Button speedUpGameButton;
    [SerializeField] private Button pauseGameButton;
    [SerializeField] private ProgressBar progressBar;
    [SerializeField] private TutorialManager tutorialManager;
    [Inject] private TutorialProgressService _tutorialProgressService;
    [Inject] private BlockManager _blockManager;
    [Inject] private PlayerRuntimeManager _playerManager;
    [Inject] private WaveManager _waveManager;
    [Inject] private GameSpeed _gameSpeed;
    private bool subscribed;
    private bool trainingSession;
    private RectTransform speedOverlay;
    private TMP_Text speedLabel;

    private void OnEnable()
    {
        if (_gameSpeed == null || subscribed) return;
        subscribed = true;
        trainingSession = _tutorialProgressService.ShouldPlayTutorial;
        ConfigureSpeedButton();
        tutorialManager.OnStateChanged += UpdateUI;
        _playerManager.OnCurrencyChanged += UpdateUI;
        _waveManager.OnTimerBetweenWavesStarted += OnTimerBetweenWavesStarted;
        _gameSpeed.OnChanged += RefreshSpeed;
        buyBlockButton.onClick.AddListener(OnBuyButtonClicked);
        buyBlockByAdButton.onClick.AddListener(OnBuyBlockByAdButtonClicked);
        speedUpGameButton.onClick.AddListener(OnSpeedUpButtonClicked);
        pauseGameButton.onClick.AddListener(pauseMenuPanel.ShowPanel);
        pauseGameButton.gameObject.SetActive(true);
        RefreshSpeed();
    }

    private void Start()
    {
        if (!subscribed) OnEnable();
        UpdateUI();
    }

    private void OnDisable()
    {
        if (!subscribed) return;
        subscribed = false;
        tutorialManager.OnStateChanged -= UpdateUI;
        _playerManager.OnCurrencyChanged -= UpdateUI;
        _waveManager.OnTimerBetweenWavesStarted -= OnTimerBetweenWavesStarted;
        _gameSpeed.OnChanged -= RefreshSpeed;
        buyBlockButton.onClick.RemoveListener(OnBuyButtonClicked);
        buyBlockByAdButton.onClick.RemoveListener(OnBuyBlockByAdButtonClicked);
        speedUpGameButton.onClick.RemoveListener(OnSpeedUpButtonClicked);
        pauseGameButton.onClick.RemoveListener(pauseMenuPanel.ShowPanel);
        if (speedOverlay != null) speedOverlay.gameObject.SetActive(false);
    }

    private void ConfigureSpeedButton()
    {
        if (speedOverlay != null) return;
        speedOverlay = RuntimeUI.Overlay("Gameplay speed", null, 30);
        var rect = speedUpGameButton.GetComponent<RectTransform>();
        rect.SetParent(speedOverlay, false);
        rect.anchorMin = rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(-118f, -225f);
        rect.sizeDelta = new Vector2(176f, 104f);
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
        var image = speedUpGameButton.GetComponent<Image>();
        image.sprite = null;
        image.type = Image.Type.Simple;
        image.preserveAspect = false;
        image.alphaHitTestMinimumThreshold = 0f;
        image.raycastTarget = true;
        image.raycastPadding = Vector4.zero;
        image.color = new Color(0.12f, 0.19f, 0.24f, 0.97f);
        speedUpGameButton.targetGraphic = image;
        foreach (var graphic in speedUpGameButton.GetComponentsInChildren<Graphic>(true))
            if (graphic != image)
            {
                graphic.raycastTarget = false;
                if (graphic is Image) graphic.enabled = false;
            }
        speedLabel = speedUpGameButton.GetComponentInChildren<TMP_Text>(true);
        var labelRect = speedLabel.rectTransform;
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.anchoredPosition = Vector2.zero;
        labelRect.sizeDelta = Vector2.zero;
        labelRect.localScale = Vector3.one;
        labelRect.localRotation = Quaternion.identity;
        speedLabel.fontSize = 48f;
        speedLabel.alignment = TextAlignmentOptions.Center;
        speedUpGameButton.gameObject.SetActive(true);
    }

    private void RefreshSpeed()
    {
        if (speedLabel == null) return;
        speedLabel.text = $"{_gameSpeed.Multiplier:0.#}x";
        bool preparing = tutorialManager.IsRunning || (trainingSession && !_waveManager.HasStarted);
        speedOverlay.gameObject.SetActive(!preparing && !_gameSpeed.IsPaused);
        speedUpGameButton.interactable = !_gameSpeed.IsPaused;
    }

    private void OnTimerBetweenWavesStarted(int waveIndex, float duration)
    {
        if (!trainingSession && waveIndex % 2 == 0) abilityPanel.ShowPanel();
    }

    private void UpdateUI()
    {
        playerCoins.text = _playerManager.GetCurrency(CurrencyType.Coins).ToString();
        buyBlockText.text = _blockManager.BlockPrice.ToString();
        buyBlockButton.interactable = _playerManager.GetCurrency(CurrencyType.Coins) >= _blockManager.BlockPrice &&
            _blockManager.CanCreateBlock() && tutorialManager.CanBuyBlocks;
        buyBlockByAdButton.gameObject.SetActive(!tutorialManager.IsRunning && !(trainingSession && !_waveManager.HasStarted));
        RefreshSpeed();
    }

    private void OnBuyButtonClicked()
    {
        if (tutorialManager.CanBuyBlocks && _playerManager.GetCurrency(CurrencyType.Coins) >= _blockManager.BlockPrice && _blockManager.CanCreateBlock())
        {
            _playerManager.TrySpend(CurrencyType.Coins, _blockManager.BlockPrice);
            _blockManager.CreateBlock();
        }
        UpdateUI();
    }

    private void OnBuyBlockByAdButtonClicked()
    {
        if (tutorialManager.CanBuyBlocks) _blockManager.CreateBlock(true);
        UpdateUI();
    }

    private void OnSpeedUpButtonClicked() => _gameSpeed.Toggle();

    public void ShowWinPanel()
    {
        tutorialManager.HideTutorial();
        winPanel.ShowPanel();
    }

    public void ShowLosePanel()
    {
        tutorialManager.HideTutorial();
        losePanel.ShowPanel();
    }
}
