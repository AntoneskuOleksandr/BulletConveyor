using UnityEngine;
using UnityEngine.UI;
using Zenject;
using TMPro;

public class WeaponsPanel : MonoBehaviour
{
    [Header("References")]
    [Inject] private WeaponsCarousel weaponsCarousel;
    [Inject] private UpgradeWeaponPanel upgradeWeaponPanel;
    [Inject] private WeaponsService _weaponsService;

    [Header("UI Elements")]
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Button selectButton;
    [SerializeField] private TMP_Text selectButtonText;

    [SerializeField] private Button nextWeaponButton;
    [SerializeField] private Button previousWeaponButton;

    private void OnEnable()
    {
        nextWeaponButton.onClick.AddListener(() => weaponsCarousel.Next());
        previousWeaponButton.onClick.AddListener(() => weaponsCarousel.Prev());
        upgradeButton.onClick.AddListener(() => upgradeWeaponPanel.OnUpgradeButtonClick());
        selectButton.onClick.AddListener(SelectWeapon);

        weaponsCarousel.OnWeaponChanged += OnWeaponChanged;
        _weaponsService.OnSelectionChanged += RefreshSelectButton;
        _weaponsService.OnWeaponsChanged += RefreshSelectButton;

        OnWeaponChanged(weaponsCarousel.CurrentWeapon);
    }

    private void OnDisable()
    {
        nextWeaponButton.onClick.RemoveAllListeners();
        previousWeaponButton.onClick.RemoveAllListeners();
        upgradeButton.onClick.RemoveAllListeners();
        selectButton.onClick.RemoveAllListeners();

        weaponsCarousel.OnWeaponChanged -= OnWeaponChanged;
        _weaponsService.OnSelectionChanged -= RefreshSelectButton;
        _weaponsService.OnWeaponsChanged -= RefreshSelectButton;
    }

    private void OnDestroy()
    {
        weaponsCarousel.OnWeaponChanged -= OnWeaponChanged;
        _weaponsService.OnSelectionChanged -= RefreshSelectButton;
        _weaponsService.OnWeaponsChanged -= RefreshSelectButton;
    }

    private void OnWeaponChanged(WeaponSO weapon)
    {
        RefreshSelectButton();
    }

    private void SelectWeapon()
    {
        var weapon = weaponsCarousel.CurrentWeapon;
        if (weapon == null) return;

        int id = weapon.WeaponId;

        if (_weaponsService.IsSelected(id))
        {
            _weaponsService.DeselectWeapon(id);
        }
        else
        {
            bool success = _weaponsService.SelectWeapon(id);
            if (!success)
            {
                Debug.LogError("Failed to select weapon. Maybe slots are full?");
            }
        }

        RefreshSelectButton();
    }

    private void RefreshSelectButton()
    {
        var weapon = weaponsCarousel.CurrentWeapon;
        if (weapon == null)
        {
            selectButton.interactable = false;
            if (selectButtonText != null) selectButtonText.text = "Select";
            return;
        }

        int id = weapon.WeaponId;
        bool isSelected = _weaponsService.IsSelected(id);
        int selectedCount = _weaponsService.GetSelectedCount();
        int maxSlots = _weaponsService.GetMaxSlots();

        if (isSelected)
        {
            selectButton.interactable = true;
            if (selectButtonText != null) selectButtonText.text = "Deselect";
        }
        else
        {
            bool canSelect = selectedCount < maxSlots;
            selectButton.interactable = canSelect;
            if (selectButtonText != null) selectButtonText.text = canSelect ? "Select" : $"Full ({selectedCount}/{maxSlots})";
        }
    }
}
