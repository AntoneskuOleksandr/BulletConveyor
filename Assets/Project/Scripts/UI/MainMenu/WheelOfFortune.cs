using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class WheelOfFortune : MonoBehaviour
{
    [SerializeField] private Transform wheel;
    [SerializeField] private int numberOfGifts;
    [SerializeField] private float timeRotate;
    [SerializeField] private float numberCircleRotate;
    [SerializeField] private AnimationCurve curve;
    [SerializeField] private int[] gifts;
    [SerializeField] private string[] giftsText;
    [SerializeField] private TMP_Text[] giftsValueText;
    [SerializeField] private Button wheelButton;
    [SerializeField] private Button spinButton;
    [SerializeField] private Button claimButton;
    [SerializeField] private TMP_Text giftText;
    [SerializeField] private float wheelIntervalMinutes = 20f;
    [SerializeField] private TMP_Text timerText;

    private MainMenuManager menuManager;
    private DateTime nextWheelTime;
    private const float CIRCLE = 360f;
    private float angleOfOneGift;
    private float currentTime;
    private int indexGiftRandom;
    private bool isWheelScreen;

    private void Start()
    {
        menuManager = GetComponent<MainMenuManager>();
        angleOfOneGift = CIRCLE / numberOfGifts;

        string nextWheelTimeStr = PlayerPrefs.GetString("NextWheelTime", string.Empty);
        if (!string.IsNullOrEmpty(nextWheelTimeStr))
            nextWheelTime = DateTime.Parse(nextWheelTimeStr);

        for (int i = 0; i < gifts.Length; i++)
        {
            giftsText[i] = gifts[i].ToString();
            giftsValueText[i].text = gifts[i].ToString();
        }

        bool isButtonInteractable = PlayerPrefs.GetInt("IsButtonInteractable", 1) > 0;
        wheelButton.interactable = isButtonInteractable;

        StartCoroutine(WheelStateUpdater());
    }

    private IEnumerator WheelStateUpdater()
    {
        while (true)
        {
            UpdateWheelState();
            yield return new WaitForSeconds(1);
        }
    }

    private void UpdateWheelState()
    {
        DateTime localTime = DateTime.Now;

        if (localTime >= nextWheelTime)
        {
            wheelButton.interactable = true;
        }

        TimeSpan timeToWheel = nextWheelTime - localTime;
        if (timeToWheel.TotalSeconds > 0)
        {
            wheelButton.interactable = false;
            timerText.text = string.Format("{0:D2}:{1:D2}:{2:D2}", timeToWheel.Hours, timeToWheel.Minutes, timeToWheel.Seconds);
        }
        else
        {
            timerText.text = "SPIN!!!";
        }
    }

    public void Rotate()
    {
        StartCoroutine(RotateWheel());
    }

    private IEnumerator RotateWheel()
    {
        spinButton.gameObject.SetActive(false);

        PlayerPrefs.SetInt("IsButtonInteractable", 0);
        nextWheelTime = DateTime.Now.AddMinutes(wheelIntervalMinutes);
        PlayerPrefs.SetString("NextWheelTime", nextWheelTime.ToString());

        float startAngle = wheel.transform.eulerAngles.z;
        currentTime = 0;
        indexGiftRandom = Random.Range(0, numberOfGifts);

        float angleWant = (numberCircleRotate * CIRCLE) + angleOfOneGift * indexGiftRandom - startAngle;

        while (currentTime < timeRotate)
        {
            yield return new WaitForEndOfFrame();
            currentTime += Time.deltaTime;

            float angleCurrent = angleWant * curve.Evaluate(currentTime / timeRotate);
            wheel.transform.eulerAngles = new Vector3(0, 0, angleCurrent + startAngle);
        }

        giftText.text = giftsText[indexGiftRandom];
        claimButton.gameObject.SetActive(true);
    }

    public void ClaimGift()
    {
        ChangeWheelScreenState();
        claimButton.gameObject.SetActive(false);
        giftText.text = "";
        CoinMove.Instance.CountCoins(gifts[indexGiftRandom], this.transform);
    }

    public void ChangeWheelScreenState()
    {
        isWheelScreen = !isWheelScreen;
        menuManager.ShowOrHideWheelScrene(isWheelScreen);
    }

}
