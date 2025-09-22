using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    private int CurrentStreak
    {
        get => PlayerPrefs.GetInt("currentSteak", 0);
        set => PlayerPrefs.SetInt("currentSteak", value);
    }

    private DateTime? LastClaimTime
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

        if (LastClaimTime.HasValue)
        {
            var timeSpan = DateTime.Now - LastClaimTime.Value;

            if (timeSpan.TotalHours > claimDeadline)
            {
                LastClaimTime = null;
                CurrentStreak = 0;
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
            var nextClaimTime = LastClaimTime.Value.AddHours(claimCooldown);
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
        if (!canClaimReward)
            return;

        var reward = rewards[CurrentStreak];

        LastClaimTime = DateTime.Now;
        CurrentStreak = (CurrentStreak + 1) % maxStpeakCount;

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
