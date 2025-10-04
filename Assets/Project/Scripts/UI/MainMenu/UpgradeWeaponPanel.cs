using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class UpgradeWeaponPanel : MonoBehaviour
{
    [Inject] private WeaponsService _weaponsService;
    [Inject] private EconomyService _economyService;

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

        _economyService.OnCurrencyChanged += RefreshUI;
        _weaponsService.OnWeaponsChanged += RefreshUI;
    }

    private void OnDisable()
    {
        foreach (var panel in upgradeParametrPanels)
            panel.button.onClick.RemoveAllListeners();

        _economyService.OnCurrencyChanged -= RefreshUI;
        _weaponsService.OnWeaponsChanged -= RefreshUI;

        ClosePanel();
    }

    public void OnUpgradeButtonClick()
    {
        if (isOpen) ClosePanel();
        else OpenPanel();
    }

    public void SetWeapon(WeaponSO weapon)
    {
        Debug.Log($"[UpgradeWeaponPanel] SetWeapon {weapon.WeaponId}");
        currentWeapon = weapon;
        RefreshUI();
    }

    private void Upgrade(UpgradeType upgradeType)
    {
        if (currentWeapon == null) return;

        bool success = _weaponsService.TryUpgrade(currentWeapon, upgradeType);
        if (success)
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
        if (currentWeapon == null) return;

        Debug.Log($"[UpgradeWeaponPanel] RefreshUI for weapon {currentWeapon.WeaponId}");

        var progress = _weaponsService.GetProgressRO(currentWeapon.WeaponId);

        foreach (var panel in upgradeParametrPanels)
            UpdatePanelState(panel, progress);
    }

    private void UpdatePanelState(UpgradeParametrPanel panel, WeaponProgressRO progress)
    {
        Debug.Log($"[UpgradeWeaponPanel] UpdatePanelState for {panel.upgradeType}");
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

        Debug.Log($"[UpgradeWeaponPanel] Level: {level}, Cost: {cost}, Player Currency: {_economyService.Currency}");

        panel.button.interactable = _economyService.Currency >= cost;
        panel.priceText.text = cost.ToString();

        if (panel.progressBar != null)
        {
            float fill = (float)level / currentWeapon.upgradeLevels.Length;
            panel.progressBar.fillAmount = fill;
        }
    }

    private int GetLevel(WeaponProgressRO progress, UpgradeType type)
    {
        return type switch
        {
            UpgradeType.Damage => progress.DamageLevel,
            UpgradeType.FireRate => progress.FireRateLevel,
            UpgradeType.Health => progress.HealthLevel,
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
