using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Zenject;

public class GiftUI : MonoBehaviour
{
    [Header("Gift Settings")]
    [SerializeField] private int rewardCoins = 100;
    [SerializeField] private float cooldownHours = 1f;

    [Header("UI References")]
    [SerializeField] private Button claimButton;
    [SerializeField] private TextMeshProUGUI cooldownText;

    private const string TIMER_KEY = "GiftTimer";

    [Inject] private TimerManager _timerManager;
    [Inject] private EconomyService _economyService;

    private void Awake()
    {
        _timerManager.RegisterTimer(TIMER_KEY, cooldownHours);
        claimButton.onClick.AddListener(ClaimGift);
    }

    private void Start()
    {
        StartCoroutine(UpdateCooldownRoutine());
    }

    private IEnumerator UpdateCooldownRoutine()
    {
        while (true)
        {
            UpdateUI();
            yield return new WaitForSeconds(1f);
        }
    }

    private void UpdateUI()
    {
        if (_timerManager.CanTrigger(TIMER_KEY))
        {
            claimButton.interactable = true;
            if (cooldownText != null)
                cooldownText.text = "Gift Available!";
        }
        else
        {
            claimButton.interactable = false;

            var remaining = _timerManager.GetRemaining(TIMER_KEY);
            if (cooldownText != null)
                cooldownText.text = $"{remaining.Hours:D2}:{remaining.Minutes:D2}:{remaining.Seconds:D2}";
        }
    }

    private void ClaimGift()
    {
        if (!_timerManager.CanTrigger(TIMER_KEY)) return;

        _economyService.AddCurrency(CurrencyType.Gears, rewardCoins, true, claimButton.GetComponent<RectTransform>());

        _timerManager.Trigger(TIMER_KEY);
        UpdateUI();
    }

}
