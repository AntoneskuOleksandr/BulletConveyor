using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class DailyRewards : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI status;
    [SerializeField] private GameObject dailyRewardPanel;
    [SerializeField] private Button claimButton;
    [SerializeField] private Button openButton;
    [SerializeField] private RewardPrefab rewardPrefab;
    [SerializeField] private Transform rewardsGrid;

    [Header("Rewards Settings")]
    [SerializeField] private List<Reward> rewards;
    [SerializeField] private float claimCooldown = 24f; // кулдаун (часы)
    [SerializeField] private float claimDeadline = 48f; // дедлайн (часы)

    private const string TIMER_KEY = "DailyReward";
    private int maxStreakCount = 8;
    private bool canClaimReward;

    private List<RewardPrefab> rewardPrefabs;

    [Inject] private TimerManager _timerManager;
    [Inject] private EconomyManager _economyManager;

    private int CurrentStreak
    {
        get => PlayerPrefs.GetInt("currentStreak", 0);
        set => PlayerPrefs.SetInt("currentStreak", value);
    }

    private void OnEnable()
    {
        openButton.onClick.AddListener(OpenDailyRewardPanel);
        claimButton.onClick.AddListener(ClaimReward);
    }

    private void OnDisable()
    {
        openButton.onClick.RemoveListener(OpenDailyRewardPanel);
        claimButton.onClick.RemoveListener(ClaimReward);
    }

    private void Start()
    {
        _timerManager.RegisterTimer(TIMER_KEY, claimCooldown);

        InitPrefabs();
        StartCoroutine(RewardsStateUpdater());
    }

    private void InitPrefabs()
    {
        rewardPrefabs = new List<RewardPrefab>();

        for (int i = 0; i < maxStreakCount; i++)
            rewardPrefabs.Add(Instantiate(rewardPrefab, rewardsGrid, false));
    }

    private IEnumerator RewardsStateUpdater()
    {
        while (true)
        {
            UpdateRewardState();
            yield return new WaitForSeconds(1);
        }
    }

    private void UpdateRewardState()
    {
        var lastTrigger = _timerManager.GetRemaining(TIMER_KEY);
        DateTime? lastClaimTime = LoadLastClaimTime();

        if (lastClaimTime.HasValue)
        {
            var span = DateTime.Now - lastClaimTime.Value;
            if (span.TotalHours > claimDeadline)
            {
                _timerManager.ResetTimer(TIMER_KEY);
                CurrentStreak = 0;
                lastClaimTime = null;
            }
        }

        canClaimReward = _timerManager.CanTrigger(TIMER_KEY);

        UpdateRewardsUI(lastClaimTime);
    }

    private void UpdateRewardsUI(DateTime? lastClaimTime)
    {
        claimButton.interactable = canClaimReward;

        if (canClaimReward)
        {
            status.text = "Claim your reward!";
        }
        else if (lastClaimTime.HasValue)
        {
            var nextClaimTime = lastClaimTime.Value.AddHours(claimCooldown);
            var currentClaimCooldown = nextClaimTime - DateTime.Now;

            string cd = $"{currentClaimCooldown.Hours:D2}:{currentClaimCooldown.Minutes:D2}:{currentClaimCooldown.Seconds:D2}";
            status.text = $"Come back in {cd} for your next reward";
        }

        for (int i = 0; i < rewardPrefabs.Count; i++)
        {
            bool isClaimed = i < CurrentStreak;
            rewardPrefabs[i].SetRewardData(i, CurrentStreak, rewards[i], isClaimed);
        }
    }

    public void ClaimReward()
    {
        if (!canClaimReward) return;

        var reward = rewards[CurrentStreak];

        int rewardIndex = rewards.IndexOf(reward);

        if (reward.type == Reward.RewardType.COIN)
        {
            _economyManager.AddMoney(reward.value, rewardPrefabs[rewardIndex].GetComponent<RectTransform>(), true);
        }

        _timerManager.Trigger(TIMER_KEY);
        SaveLastClaimTime(DateTime.Now);

        CurrentStreak = (CurrentStreak + 1) % maxStreakCount;

        UpdateRewardState();
        CloseDailyRewardPanel();
    }

    public void OpenDailyRewardPanel()
    {
        dailyRewardPanel.SetActive(true);
    }

    public void CloseDailyRewardPanel()
    {
        dailyRewardPanel.SetActive(false);
    }

    private void SaveLastClaimTime(DateTime time)
    {
        PlayerPrefs.SetString("DailyReward_LastClaimTime", time.ToString());
    }

    private DateTime? LoadLastClaimTime()
    {
        string data = PlayerPrefs.GetString("DailyReward_LastClaimTime", "");
        if (!string.IsNullOrEmpty(data))
            return DateTime.Parse(data);
        return null;
    }
}
