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
    [Inject] private WeaponsCarousel _weaponsCarousel;

    [Header("Upgrade Panels")]
    [SerializeField] private List<UpgradeParametrPanel> upgradeParametrPanels;

    private WeaponSO currentWeapon;
    private bool isOpen = false;
    private readonly Dictionary<UpgradeParametrPanel, UnityEngine.Events.UnityAction> handlers = new();

    private void OnEnable()
    {
        foreach (var panel in upgradeParametrPanels)
        {
            var capturedType = panel.upgradeType;
            UnityEngine.Events.UnityAction handler = () => Upgrade(capturedType);
            handlers[panel] = handler;
            panel.button.onClick.AddListener(handler);
        }

        _economyService.OnCurrencyChanged += OnCurrencyChanged;
        _weaponsService.OnWeaponsChanged += RefreshUI;

        _weaponsCarousel.OnWeaponChanged += SetWeapon;
        SetWeapon(_weaponsCarousel.CurrentWeapon);

        RefreshUI();
    }

    private void OnDisable()
    {
        foreach (var panel in upgradeParametrPanels)
            if (handlers.TryGetValue(panel, out var handler)) panel.button.onClick.RemoveListener(handler);
        handlers.Clear();

        _economyService.OnCurrencyChanged -= OnCurrencyChanged;
        _weaponsCarousel.OnWeaponChanged -= SetWeapon;
        _weaponsService.OnWeaponsChanged -= RefreshUI;

        ClosePanel();
    }

    private void OnCurrencyChanged(CurrencyType type) => RefreshUI();

    public void OnUpgradeButtonClick()
    {
        if (isOpen) ClosePanel();
        else OpenPanel();
    }

    public void SetWeapon(WeaponSO weapon)
    {
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

        var progress = _weaponsService.GetProgressRO(currentWeapon.WeaponId);

        foreach (var panel in upgradeParametrPanels)
            UpdatePanelState(panel, progress);
    }

    private void UpdatePanelState(UpgradeParametrPanel panel, WeaponProgressRO progress)
    {
        int level = GetLevel(progress, panel.upgradeType);

        if (level >= currentWeapon.UpgradeLevels.Length)
        {
            panel.button.interactable = false;
            panel.priceText.text = "MAX";
            if (panel.progressBar != null)
                panel.progressBar.fillAmount = 1f;
            return;
        }

        int cost = currentWeapon.UpgradeLevels[level].Cost;

        panel.button.interactable = _economyService.GetCurrency(CurrencyType.Gears) >= cost;
        panel.priceText.text = cost.ToString();

        if (panel.progressBar != null)
        {
            float fill = (float)level / currentWeapon.UpgradeLevels.Length;
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
