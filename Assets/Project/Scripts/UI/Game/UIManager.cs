using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
public class UIManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private AbilityPanel abilityPanel;

    [Header("Text")]
    [SerializeField] private TMP_Text playerMoney;
    [SerializeField] private TMP_Text buyBlockText;

    [Header("Buttons")]
    [SerializeField] private Button buyBlockButton;
    [SerializeField] private Button buyBlockByAdButton;
    [SerializeField] private Button speedUpGameButton;

    [SerializeField] private float timeSpeedUpValue = 3f;

    [Header("Progress Bar")]
    [SerializeField] private ProgressBar progressBar;

    [Inject] private BlockManager _blockManager;
    [Inject] private PlayerManager _playerManager;
    [Inject] private WaveManager _waveManager;

    private void OnEnable()
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
            _playerManager.OnMoneyChanged += UpdateUI;
        }
        if (_waveManager != null && abilityPanel != null)
        {
            _waveManager.OnTimerBetweenWavesStarted += (waveIndex, timeBetweenWaves) =>
            {
                if (waveIndex % 2 == 0)
                {
                    abilityPanel.ShowPanel();
                }
            };
        }
    }

    private void Start()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        playerMoney.text = _playerManager.Money.ToString();
        buyBlockText.text = _blockManager.BlockPrice.ToString();
        buyBlockButton.interactable = _playerManager.Money >= _blockManager.BlockPrice;
    }

    private void OnBuyButtonClicked()
    {
        if (_playerManager.Money >= _blockManager.BlockPrice)
        {
            _playerManager.SpendMoney(_blockManager.BlockPrice);
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
}