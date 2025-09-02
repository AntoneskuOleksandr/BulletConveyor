using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
public class UIManager : MonoBehaviour
{
    [SerializeField] private Button buyBlockByAdButton;
    [SerializeField] private Button speedUpGameButton;

    [SerializeField] private float timeSpeedUpValue = 3f;

    [Header("Progress Bar")]
    [SerializeField] private ProgressBar progressBar;

    [Inject] private BlockManager _blockManager;

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
        float newTimeSpeedValue = (Time.timeScale == timeSpeedUpValue) ? 1f : timeSpeedUpValue;

        speedUpGameButton.GetComponentInChildren<TMP_Text>().text = newTimeSpeedValue.ToString();
        Time.timeScale = newTimeSpeedValue;
    }
}