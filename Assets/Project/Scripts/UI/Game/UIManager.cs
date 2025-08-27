using UnityEngine;
using UnityEngine.UI;
using Zenject;
public class UIManager : MonoBehaviour
{
    [SerializeField] private Button buyBlockByAdButton;
    [SerializeField] private Button speedUpGameButton;

    [Header("Progress Bar")]
    [SerializeField] private ProgressBar progressBar;

    [Inject] private BlockManager _blockManager;
    [Inject] private WaveManager _waveManager;

    private void Awake()
    {
        if (buyBlockByAdButton != null)
        {
            buyBlockByAdButton.onClick.AddListener(OnBuyBlockByAdButtonClicked);
        }
        if (speedUpGameButton != null)
        {
            speedUpGameButton.onClick.AddListener(OnSpeedUpButtonClicked);
        }
    }

    private void OnBuyBlockByAdButtonClicked()
    {
        Debug.Log("Buy Block by Ad button clicked.");
        if (_blockManager != null)
        {
            _blockManager.CreateBlock(0); // Assuming 0 is the index of the block to create
        }
        else
        {
            Debug.LogError("BlockManager is not initialized. Cannot create block.");
        }
    }

    private void OnSpeedUpButtonClicked()
    {
        Time.timeScale = 2f;
    }
}