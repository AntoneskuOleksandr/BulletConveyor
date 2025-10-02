using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class UpgradeWeaponPanel : MonoBehaviour
{
    [Inject] private DataManager _dataManager;
    [Inject] private EconomyManager _economyManager;

    [Header("Upgrade Panels")]
    [SerializeField] private List<UpgradeParametrPanel> upgradeParametrPanels;

    private WeaponSO currentWeapon;
    private bool isOpen = false;

    private void OnEnable()
    {
        foreach (var panel in upgradeParametrPanels)
        {
            var capturedType = panel.upgradeType;
            panel.button.onClick.AddListener(() => Upgrade(capturedType));
        }

        _economyManager.OnCurrencyChanged += RefreshUI;
    }

    private void OnDisable()
    {
        foreach (var panel in upgradeParametrPanels)
        {
            panel.button.onClick.RemoveAllListeners();
        }

        _economyManager.OnCurrencyChanged -= RefreshUI;

        ClosePanel();
    }

    public void OnUpgradeButtonClick()
    {
        if (isOpen)
            ClosePanel();
        else
            OpenPanel();
    }

    public void SetWeapon(WeaponSO weapon)
    {
        currentWeapon = weapon;
        RefreshUI();
    }

    private void Upgrade(UpgradeType upgradeType)
    {
        var progress = _dataManager.PlayerData.GetWeaponProgress(currentWeapon.WeaponId);

        int level = GetLevel(progress, upgradeType);
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
        foreach (var panel in upgradeParametrPanels)
            UpdatePanelState(panel);
    }

    private void UpdatePanelState(UpgradeParametrPanel panel)
    {
        var progress = _dataManager.PlayerData.GetWeaponProgress(currentWeapon.WeaponId);
        int level = GetLevel(progress, panel.upgradeType);

        if (level >= currentWeapon.upgradeLevels.Length)
        {
            panel.button.interactable = false;
            panel.priceText.text = "MAX";
            if (panel.progressBar != null)
                panel.progressBar.fillAmount = 1f;
            return;
        }

        int cost = currentWeapon.upgradeLevels[level].cost;

        panel.button.interactable = _dataManager.PlayerData.Currency >= cost;
        panel.priceText.text = cost.ToString();

        if (panel.progressBar != null)
        {
            float fill = (float)level / currentWeapon.upgradeLevels.Length;
            panel.progressBar.fillAmount = fill;
        }
    }

    private int GetLevel(WeaponProgress progress, UpgradeType type)
    {
        return type switch
        {
            UpgradeType.Damage => progress.damageLevel,
            UpgradeType.FireRate => progress.fireRateLevel,
            UpgradeType.Health => progress.healthLevel,
            _ => 0
        };
    }
}

[Serializable]
public class UpgradeParametrPanel
{
    public UpgradeType upgradeType;
    public Button button;
    public TMP_Text priceText;
    public Image progressBar;
}
