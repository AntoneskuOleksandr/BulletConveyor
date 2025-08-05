using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Coins && Price Button")]
    public CoinManager coinManager;
    public TMP_Text coinsText;
    public TMP_Text coinsRewardText;
    [Range(0, 1000)][SerializeField] private float blockPrice = 50;
    [SerializeField] private Button buyButton;

    [Header("Screens")]
    [SerializeField] private GameObject inGameScreen;
    [SerializeField] private GameObject loserScreen;
    [SerializeField] private GameObject winnerScreen;
    [SerializeField] private GameObject awardTextScreen;
    [SerializeField] private GameObject inGameMenuScreen;
    [SerializeField] private GameObject abilityScreen;
    [SerializeField] private GameObject tutorialEndScreen;

    [Header("Progress Bar")]
    [SerializeField] private ProgressBar progressBar;

    [Header("Loser Screen Continue Button")]
    [SerializeField] private Button continueButton;

    [Header("Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button menuButton;
    [SerializeField] private Button multiplyRewardButton;
    [SerializeField] private Button blockForAdButton;

    [Header("Ability Buttons & Texts")]
    [SerializeField] private Button abilityButton1;
    [SerializeField] private Button abilityButton2;
    [SerializeField] private Button abilityButton3;
    [SerializeField] private TMP_Text abilityText1;
    [SerializeField] private TMP_Text abilityText2;
    [SerializeField] private TMP_Text abilityText3;

    [Header("Ability Icons")]
    [SerializeField] private Sprite fireBulletIcon;
    [SerializeField] private Sprite freezeBulletIcon;
    [SerializeField] private Sprite deadBulletIcon;
    [SerializeField] private Sprite poisonBulletIcon;
    [SerializeField] private Sprite stunBulletIcon;
    [SerializeField] private Sprite damageInceasedBulletIcon;
    [SerializeField] private Sprite explosionBulletIcon;
    [SerializeField] private Sprite fireInRadiusIcon;
    [SerializeField] private Sprite freezeInRadiusIcon;
    [SerializeField] private Sprite poisonInRadiusIcon;
    [SerializeField] private Sprite addBulletToGenerateIcon;
    [SerializeField] private List<Image> abilityImages;
    [SerializeField] private List<Button> unlockTileButtons;

    [Header("Scene")]
    [SerializeField] private string mainMenuScene = "MainMenu";

    [Header("Texts")]
    [SerializeField] private TMP_Text wavesComingText;

    private TMP_Text buyButtonText;
    private AbilityManager abilityManager;


    private void Awake()
    {
        Instance = this;

        resumeButton.onClick.AddListener(ResumeGame);
        menuButton.onClick.AddListener(inGameMenu);

        buyButtonText = buyButton.GetComponentInChildren<TMP_Text>();

        coinManager.onCoinsChanged.AddListener(UpdateUI);

        if (GameManager.Instance.isTutorial)
            GameManager.Instance.onMerge.AddListener(ShowTutorialEndScreen);
    }

    private void Start()
    {
        abilityManager = AbilityManager.Instance;
        Spawner.Instance.OnEnemyDeath.AddListener(AddCoinsForEnemyDeath);
    }

    public void UpdateUI()
    {
        coinsText.text = coinManager.Coins.ToString();
        buyButton.interactable = coinManager.Coins >= blockPrice && GameManager.Instance.CanCreateBlock();
        buyButtonText.text = blockPrice.ToString();
        if (GameManager.Instance.isTutorial)
        {
            if (coinManager.Coins >= blockPrice)
                GameManager.Instance.onEnoughMoneyToBuyBlock.Invoke();
        }
        else
            blockForAdButton.interactable = GameManager.Instance.CanCreateBlock();


        if (GameManager.Instance.isTutorial)
            return;

        foreach (var button in unlockTileButtons)
        {
            UnlockTileButton unlockTileButton = button.GetComponent<UnlockTileButton>();

            if (unlockTileButton.useGameCurrency)
                button.interactable = PlayerPrefs.GetFloat("Coins", 0) >= unlockTileButton.unlockPrice;
            else
                button.interactable = coinManager.Coins >= unlockTileButton.unlockPrice;
        }
    }

    public void UpdateProgressBar()
    {
        progressBar.StartProgressBar();
    }

    public void ChangeWaveSprite()
    {
        progressBar.ChangeWaveSprite();
    }

    private void AddCoinsForEnemyDeath()
    {
        coinManager.AddCoins(5);
    }

    public void ResumeGame()
    {
        inGameScreen.SetActive(true);
        inGameMenuScreen.SetActive(false);

        Time.timeScale = 1f;
    }

    public void inGameMenu()
    {
        inGameScreen.SetActive(false);
        inGameMenuScreen.SetActive(true);
        winnerScreen.SetActive(false);

        ShowSelectedAbilities();

        Time.timeScale = 0f;
    }

    public void MainMenu()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(mainMenuScene);
    }

    public IEnumerator ShowWinerScreen()
    {
        yield return new WaitForSeconds(3f);

        awardTextScreen.SetActive(true);
        winnerScreen.SetActive(true);
        inGameScreen.SetActive(false);

        Time.timeScale = 0f;
    }

    public void ShowLoserScreen()
    {
        Time.timeScale = 0f;

        awardTextScreen.SetActive(true);
        loserScreen.SetActive(true);
        inGameScreen.SetActive(false);
        StartCoroutine(DeactivateContinueButton());
    }

    public void CoinsReward(float coinsReward)
    {
        coinsRewardText.text = coinsReward + " COINS";
    }

    public void ShowAbilities()
    {
        List<AbilityManager.Ability> randomAbilities = abilityManager.GetRandomAbilities(3);
        Time.timeScale = 0f;
        inGameScreen.SetActive(false);
        abilityScreen.SetActive(true);

        UpdateAbilityButton(abilityButton1, abilityText1, randomAbilities[0]);
        UpdateAbilityButton(abilityButton2, abilityText2, randomAbilities[1]);
        UpdateAbilityButton(abilityButton3, abilityText3, randomAbilities[2]);
    }

    private void UpdateAbilityButton(Button button, TMP_Text text, AbilityManager.Ability ability)
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => abilityManager.SelectAbility(ability));

        Image icon = button.GetComponent<Image>();
        switch (ability)
        {
            case AbilityManager.Ability.FireBullet:
                icon.sprite = fireBulletIcon;
                text.text = AbilityProbabilityManager.Instance.GetProbability(AbilityManager.Ability.FireBullet) * 100 + "% chance for a fire shot";
                break;
            case AbilityManager.Ability.FreezeBullet:
                icon.sprite = freezeBulletIcon;
                text.text = AbilityProbabilityManager.Instance.GetProbability(AbilityManager.Ability.FreezeBullet) * 100 + "% chance for a freezing shot";
                break;
            case AbilityManager.Ability.DeadBullet:
                icon.sprite = deadBulletIcon;
                text.text = AbilityProbabilityManager.Instance.GetProbability(AbilityManager.Ability.DeadBullet) * 100 + "% chance for a dead shot";
                break;
            case AbilityManager.Ability.PoisonBullet:
                icon.sprite = poisonBulletIcon;
                text.text = AbilityProbabilityManager.Instance.GetProbability(AbilityManager.Ability.PoisonBullet) * 100 + "% chance for a poison shot";
                break;
            case AbilityManager.Ability.StunBullet:
                icon.sprite = stunBulletIcon;
                text.text = AbilityProbabilityManager.Instance.GetProbability(AbilityManager.Ability.StunBullet) * 100 + "% chance for a stun shot";
                break;
            case AbilityManager.Ability.DamageIncreasedBullet:
                icon.sprite = damageInceasedBulletIcon;
                text.text = AbilityProbabilityManager.Instance.GetProbability(AbilityManager.Ability.DamageIncreasedBullet) * 100 + "% chance for a increased damage shot";
                break;
            case AbilityManager.Ability.ExlosionBullet:
                icon.sprite = explosionBulletIcon;
                text.text = AbilityProbabilityManager.Instance.GetProbability(AbilityManager.Ability.ExlosionBullet) * 100 + "% chance for explosion shot";
                break;
            case AbilityManager.Ability.FireInRadius:
                icon.sprite = fireInRadiusIcon;
                text.text = AbilityProbabilityManager.Instance.GetProbability(AbilityManager.Ability.FireInRadius) * 100 + "% chance that after death the enemy will set other enemies on fire";
                break;
            case AbilityManager.Ability.FreezeInRadius:
                icon.sprite = freezeInRadiusIcon;
                text.text = AbilityProbabilityManager.Instance.GetProbability(AbilityManager.Ability.FreezeInRadius) * 100 + "% chance that the enemy will freeze other enemies after death";
                break;
            case AbilityManager.Ability.PoisonInRadius:
                icon.sprite = poisonInRadiusIcon;
                text.text = AbilityProbabilityManager.Instance.GetProbability(AbilityManager.Ability.PoisonInRadius) * 100 + "% chance that the enemy will poison other enemies after death";
                break;
            case AbilityManager.Ability.AddBulletsToGenerate:
                icon.sprite = addBulletToGenerateIcon;
                text.text = "+1 bullet for each block";
                break;
        }
    }

    public void HideAbilityScreen()
    {
        Time.timeScale = 1f;
        inGameScreen.SetActive(true);
        abilityScreen.SetActive(false);
    }

    public void ShowSelectedAbilities()
    {
        for (int i = 0; i < abilityManager.selectedAbilities.Count; i++)
        {
            UpdateAbilityImage(abilityImages[i], abilityManager.selectedAbilities[i]);
        }
    }

    private void UpdateAbilityImage(Image image, AbilityManager.Ability ability)
    {
        image.gameObject.SetActive(true);

        switch (ability)
        {
            case AbilityManager.Ability.FireBullet:
                image.sprite = fireBulletIcon;
                break;
            case AbilityManager.Ability.FreezeBullet:
                image.sprite = freezeBulletIcon;
                break;
            case AbilityManager.Ability.DeadBullet:
                image.sprite = deadBulletIcon;
                break;
            case AbilityManager.Ability.PoisonBullet:
                image.sprite = poisonBulletIcon;
                break;
            case AbilityManager.Ability.StunBullet:
                image.sprite = stunBulletIcon;
                break;
            case AbilityManager.Ability.DamageIncreasedBullet:
                image.sprite = damageInceasedBulletIcon;
                break;
            case AbilityManager.Ability.ExlosionBullet:
                image.sprite = explosionBulletIcon;
                break;
            case AbilityManager.Ability.FireInRadius:
                image.sprite = fireInRadiusIcon;
                break;
            case AbilityManager.Ability.FreezeInRadius:
                image.sprite = freezeInRadiusIcon;
                break;
            case AbilityManager.Ability.PoisonInRadius:
                image.sprite = poisonInRadiusIcon;
                break;
            case AbilityManager.Ability.AddBulletsToGenerate:
                image.sprite = addBulletToGenerateIcon;
                break;
        }
    }

    public void OnBuyButton()
    {
        if (coinManager.Coins >= blockPrice)
        {
            coinManager.SpendCoins(blockPrice);
            GameManager.Instance.CreateBlock();

            if (GameManager.Instance.isTutorial)
                GameManager.Instance.onBlockBuy.Invoke();

            blockPrice *= 2;
            buyButtonText.text = blockPrice.ToString();
        }
    }

    public void OnRetryButton()
    {
        GameManager.Instance.Retry();
    }

    public void OnContinueButton()
    {
        loserScreen.SetActive(false);
        awardTextScreen.SetActive(false);
        inGameScreen.SetActive(true);

        GameManager.Instance.ContinueForAd();
        UpdateUI();
    }

    public void OnNextLevelButton()
    {
        Time.timeScale = 1f;
        GameManager.Instance.LoadNextLevel();
        UpdateUI();
    }

    public void HideMultiplyRewardButton()
    {
        multiplyRewardButton.gameObject.SetActive(false);
    }

    public void ChangeWaveComingText(string text)
    {
        wavesComingText.text = text;
    }

    public IEnumerator WaveCountdownText(float countdownTime)
    {
        while (countdownTime > 0)
        {
            wavesComingText.text = $"Wave starting in <color=#00ffff>{countdownTime:F1} secs</color>";
            countdownTime -= Time.deltaTime;
            yield return null;
        }

        wavesComingText.text = "";

        UpdateProgressBar();
    }

    private IEnumerator DeactivateContinueButton()
    {
        yield return new WaitForSecondsRealtime(5f);
        continueButton.gameObject.SetActive(false);
    }

    private void ShowTutorialEndScreen()
    {
        StartCoroutine(ShowTutorialEndScreenAfterTime(3));
    }

    private IEnumerator ShowTutorialEndScreenAfterTime(float delay)
    {
        yield return new WaitForSeconds(delay);

        Time.timeScale = 0f;
        inGameScreen.SetActive(false);
        tutorialEndScreen.SetActive(true);
        PlayerPrefs.SetInt("TutorialCompleted", 1);
    }

    private void OnDestroy()
    {
        Spawner.Instance.OnEnemyDeath.RemoveListener(AddCoinsForEnemyDeath);
        coinManager.onCoinsChanged.RemoveListener(UpdateUI);
    }
}
