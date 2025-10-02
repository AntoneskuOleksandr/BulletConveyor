using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class UpgradeWeaponPanel : MonoBehaviour
{
    [Inject] private DataManager _dataManager;
    [Inject] private EconomyManager _economyManager;

    [Header("Buttons")]
    [SerializeField] private Button damageUpgradeButton;
    [SerializeField] private Button fireRateUpgradeButton;
    [SerializeField] private Button healthUpgradeButton;

    private WeaponSO currentWeapon;
    private bool isOpen = false;

    private void Awake()
    {
        damageUpgradeButton.onClick.AddListener(() =>
        {
            Upgrade(UpgradeType.Damage);
        });
        fireRateUpgradeButton.onClick.AddListener(() =>
        {
            Upgrade(UpgradeType.FireRate);
        });
        healthUpgradeButton.onClick.AddListener(() =>
        {
            Upgrade(UpgradeType.Health);
        });
    }

    public void OnUpgradeButtonClick()
    {
        if (isOpen)
        {
            ClosePanel();
        }
        else
        {
            OpenPanel();
        }
    }

    public void SetWeapon(WeaponSO weapon)
    {
        currentWeapon = weapon;
        RefreshUI();
    }

    private void Upgrade(UpgradeType upgradeType)
    {
        var progress = _dataManager.PlayerData.GetWeaponProgress(currentWeapon.WeaponId);
        int level = upgradeType switch
        {
            UpgradeType.Damage => progress.damageLevel,
            UpgradeType.FireRate => progress.fireRateLevel,
            UpgradeType.Health => progress.healthLevel,
            _ => 0
        };

        if (level >= currentWeapon.upgradeLevels.Length)
            return;

        int cost = currentWeapon.upgradeLevels[level].cost;

        if (!_economyManager.SpendCurrency(cost))
            return;

        progress.Upgrade(upgradeType);
        _dataManager.Save();

        RefreshUI();
    }



    private void OpenPanel()
    {
        gameObject.SetActive(true);
        isOpen = true;
    }
    private void ClosePanel()
    {
        gameObject.SetActive(false);
        isOpen = false;
    }

    private void RefreshUI()
    {
        UpdateButtonState(damageUpgradeButton, UpgradeType.Damage);
        UpdateButtonState(fireRateUpgradeButton, UpgradeType.FireRate);
        UpdateButtonState(healthUpgradeButton, UpgradeType.Health);
    }

    private void UpdateButtonState(Button button, UpgradeType type)
    {
        var progress = _dataManager.PlayerData.GetWeaponProgress(currentWeapon.WeaponId);
        int level = type switch
        {
            UpgradeType.Damage => progress.damageLevel,
            UpgradeType.FireRate => progress.fireRateLevel,
            UpgradeType.Health => progress.healthLevel,
            _ => 0
        };

        if (level >= currentWeapon.upgradeLevels.Length)
        {
            button.interactable = false;
            return;
        }

        int cost = currentWeapon.upgradeLevels[level].cost;
        button.interactable = _dataManager.PlayerData.Currency >= cost;
    }

}
