using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardPrefab : MonoBehaviour
{
    [SerializeField] private Image backGround;
    [SerializeField] private TextMeshProUGUI dayText;
    [SerializeField] private TextMeshProUGUI rewardValue;
    [SerializeField] private Image rewardIcon;
    [SerializeField] private Sprite currentDayBG;
    [SerializeField] private Sprite unavailableBG;
    [SerializeField] private Sprite claimedBG; 
    [SerializeField] private Sprite rewardCoin;

    public void SetRewardData(int day, int currentStreak, Reward reward, bool isClaimed)
    {
        dayText.text = $"Day {day + 1}";
        rewardIcon.sprite = reward.type == Reward.RewardType.COIN ? rewardCoin : null;
        rewardValue.text = reward.value.ToString();

        if (isClaimed)
        {
            backGround.sprite = claimedBG;
        }
        else
        {
            backGround.sprite = day == currentStreak ? currentDayBG : unavailableBG;
        }
    }
}
