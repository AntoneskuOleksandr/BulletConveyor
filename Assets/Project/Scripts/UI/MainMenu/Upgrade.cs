using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Upgrade : MonoBehaviour
{
    [SerializeField] private string product;
    [SerializeField] private float upgradeValue;
    [SerializeField] private float upgradeCost;

    [Header("UI")]
    [SerializeField] private Button upgradeButton;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private Image filledImage;
    [SerializeField] private float divisionCount;

    [SerializeField] private GunCarousel gunCarousel;

    private float filledDivisionsCount;
    private float value;
    private string productGunName;

    private void Awake()
    {
        MainMenuManager.Instance.onCoinsChanged.AddListener(UpdateButtonsState);
    }

    private void GetValue()
    {
        if (!PlayerPrefs.HasKey("UpgradeCostStartValue"))
            PlayerPrefs.SetFloat("UpgradeCostStartValue", upgradeCost);

        productGunName = product + gunCarousel.GetCurrentGunID();

        upgradeCost = PlayerPrefs.GetFloat(productGunName + "Cost", PlayerPrefs.GetFloat("UpgradeCostStartValue"));
        priceText.text = upgradeCost.ToString();

        value = PlayerPrefs.GetFloat(productGunName);
    }

    private void GetFilledIconsCount()
    {
        filledDivisionsCount = PlayerPrefs.GetFloat(productGunName + "Icons");
    }

    public void ProductUpgrade()
    {
        filledDivisionsCount++;
        PlayerPrefs.SetFloat(productGunName + "Icons", filledDivisionsCount);

        MainMenuManager.Instance.SpendCoins(upgradeCost);

        value += upgradeValue;
        PlayerPrefs.SetFloat(productGunName, value);

        filledImage.fillAmount = filledDivisionsCount / divisionCount;

        upgradeCost += 100;
        priceText.text = upgradeCost.ToString();
        PlayerPrefs.SetFloat(productGunName + "Cost", upgradeCost);

        IconUpdate();
    }

    public void IconUpdate()
    {
        GetValue();
        GetFilledIconsCount();

        filledImage.fillAmount = filledDivisionsCount / divisionCount;

        UpdateButtonsState();
    }

    public void UpdateButtonsState()
    {
        float coins = MainMenuManager.Instance.coins;

        if (coins < upgradeCost)
        {
            upgradeButton.interactable = false;
        }
        else
        {
            upgradeButton.interactable = true;
        }

        if (filledDivisionsCount >= divisionCount)
        {
            upgradeButton.interactable = false;
            priceText.text = "MAX LEVEL";
        }
    }
}
