using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections.Generic;

public class MainMenuManager : MonoBehaviour
{
    public static MainMenuManager Instance;
    public GameObject coinsGO;
    public TMP_Text coinsText;
    public float coins;
    public int selectedGunsCount;
    public GameObject[] guns;
    public GameObject levelsParent;
    public UnityEvent onCoinsChanged;

    [Header("Screens")]
    [SerializeField] private GameObject wheelScreen;

    [Header("Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button giftButton;
    [SerializeField] private Button dailyRewardButton;
    [SerializeField] private Button prevLevelButton;
    [SerializeField] private Button nextLevelButton;

    private List<Button> allButtons = new List<Button>();

    [Header("Scripts")]
    [SerializeField] private LevelDisplay levelDisplay;

    private void Awake()
    {
        Instance = this;
        allButtons.Add(playButton);
        allButtons.Add(giftButton);
        allButtons.Add(dailyRewardButton);
        allButtons.Add(prevLevelButton);
        allButtons.Add(nextLevelButton);
    }

    private void Start()
    {
        AudioManager.Instance.PlayMusic(AudioManager.Instance.MainMenuMusic);
        coins = PlayerPrefs.GetFloat("Coins");
        selectedGunsCount = PlayerPrefs.GetInt("SelectedGunsCount");

        UpdateCoinsText();
        BindSounds();
    }

    public void AddCoins(float amount, int multiplier = 1)
    {
        coins += amount;

        coins *= multiplier;

        PlayerPrefs.SetFloat("Coins", coins);

        onCoinsChanged.Invoke();
        UpdateCoinsText();
    }

    public void SpendCoins(float amount)
    {
        coins -= amount;
        PlayerPrefs.SetFloat("Coins", coins);

        onCoinsChanged.Invoke();
        UpdateCoinsText();
    }

    public void ShowOrHideWheelScrene(bool openWheelScreen)
    {
        foreach (Button button in allButtons)
            button.gameObject.SetActive(!openWheelScreen);

        wheelScreen.SetActive(openWheelScreen);
    }

    private void UpdateCoinsText()
    {
        coinsText.text = coins.ToString();
    }

    public bool IsHasAnyGun()
    {
        return selectedGunsCount > 0;
    }

    public void ChangeSelectedGunsCount(int amount)
    {
        selectedGunsCount += amount;

        PlayerPrefs.SetInt("SelectedGunsCount", selectedGunsCount);

        levelDisplay.UpdateCurrentLevelState();
    }

    private void BindSounds()
    {
        AudioManager audioManager = AudioManager.Instance;
        UnityAction playSound = () => audioManager.PlaySFX(audioManager.buttonClickSound);

        Button[] buttons = FindObjectsOfType<Button>(true);
        foreach (Button button in buttons)
        {
            button.onClick.AddListener(playSound);
        }
    }
}
