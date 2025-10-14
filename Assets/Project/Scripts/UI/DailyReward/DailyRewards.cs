using System;
using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class DailyRewards : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI status;
    [SerializeField] private GameObject dailyReward;
    [SerializeField] private Button claimButton;
    [SerializeField] private RewardPrefab rewardPrefab;
    [SerializeField] private Transform rewardsGrid;
    [SerializeField] private List<Reward> rewards;
    [SerializeField] private float claimCooldown = 24f;
    [SerializeField] private float claimDeadline = 48f;

    private int currentStreak
    {
        get => PlayerPrefs.GetInt("currentSteak", 0);
        set => PlayerPrefs.SetInt("currentSteak", value);
    }

    private DateTime? lastClaimTime
    {
        get
        {
            string data = PlayerPrefs.GetString("lastClaimedTime", null);
            if (!string.IsNullOrEmpty(data))
                return DateTime.Parse(data);
            return null;
        }
        set
        {
            if (value != null)
                PlayerPrefs.SetString("lastClaimedTime", value.ToString());
            else
                PlayerPrefs.DeleteKey("lastClaimedTime");
        }
    }

    private bool canClaimReward;
    private int maxStpeakCount = 8;

    private List<RewardPrefab> rewardPrefabs;

    private void Start()
    {
        InitPrefabs();
        StartCoroutine(RewardsStateUpdater());
    }

    private void InitPrefabs()
    {
        rewardPrefabs = new List<RewardPrefab>();

        for (int i = 0; i < maxStpeakCount; i++)
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
        canClaimReward = true;

        if (lastClaimTime.HasValue)
        {
            var timeSpan = DateTime.Now - lastClaimTime.Value;

            if (timeSpan.TotalHours > claimDeadline)
            {
                lastClaimTime = null;
                currentStreak = 0;
            }
            else if (timeSpan.TotalHours < claimCooldown)
                canClaimReward = false;
        }

        UpdateRewardsUI();
    }

    private void UpdateRewardsUI()
    {
        claimButton.interactable = canClaimReward;

        if (canClaimReward)
            status.text = "Claim your reward!";
        else
        {
            var nextClaimTime = lastClaimTime.Value.AddHours(claimCooldown);
            var currentClaimCooldown = nextClaimTime - DateTime.Now;

            string cd = $"{currentClaimCooldown.Hours:D2}:{currentClaimCooldown.Minutes:D2}:{currentClaimCooldown.Seconds:D2}";

            status.text = $"Come back in {cd} for your next reward";
        }

        for (int i = 0; i < rewardPrefabs.Count; i++)
        {
            bool isClaimed = i < currentStreak;
            rewardPrefabs[i].SetRewardData(i, currentStreak, rewards[i], isClaimed);
        }
    }

    public void ClaimReward()
    {
        if (!canClaimReward)
            return;

        var reward = rewards[currentStreak];

        CoinMove.Instance.CountCoins(reward.value, rewardsGrid.GetChild(currentStreak).transform);

        lastClaimTime = DateTime.Now;
        currentStreak = (currentStreak + 1) % maxStpeakCount;

        UpdateRewardState();

        CloseDailyRewardPanel();
    }

    public void OpenDailyRewardPanel()
    {
        dailyReward.SetActive(true);
    }

    public void CloseDailyRewardPanel()
    {
        dailyReward.SetActive(false);
    }
}
