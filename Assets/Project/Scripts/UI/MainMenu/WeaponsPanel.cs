using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class WeaponsPanel : MonoBehaviour
{
    [Header("References")]
    [Inject] private WeaponsCarousel _weaponsCarousel;
    [Inject] private UpgradeWeaponPanel _upgradeWeaponPanel;
    [Inject] private WeaponsService _weaponsService;
    [Inject] private MainMenuUIManager _mainMenuUIManager;

    [Header("Buttons")]
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Button selectButton;
    [SerializeField] private Button nextWeaponButton;
    [SerializeField] private Button previousWeaponButton;

    [Header("Text")]
    [SerializeField] private TMP_Text selectButtonText;

    private void OnEnable()
    {
        nextWeaponButton.onClick.AddListener(() => _weaponsCarousel.Next());
        previousWeaponButton.onClick.AddListener(() => _weaponsCarousel.Prev());
        upgradeButton.onClick.AddListener(() => _upgradeWeaponPanel.OnUpgradeButtonClick());
        selectButton.onClick.AddListener(SelectWeapon);

        _weaponsCarousel.OnWeaponChanged += OnWeaponChanged;
        _weaponsService.OnSelectionChanged += RefreshWeaponsUI;
        _weaponsService.OnWeaponsChanged += RefreshWeaponsUI;

        OnWeaponChanged(_weaponsCarousel.CurrentWeapon);
    }

    private void OnDisable()
    {
        nextWeaponButton.onClick.RemoveAllListeners();
        previousWeaponButton.onClick.RemoveAllListeners();
        upgradeButton.onClick.RemoveAllListeners();
        selectButton.onClick.RemoveAllListeners();

        _weaponsCarousel.OnWeaponChanged -= OnWeaponChanged;
        _weaponsService.OnSelectionChanged -= RefreshWeaponsUI;
        _weaponsService.OnWeaponsChanged -= RefreshWeaponsUI;
    }

    private void OnDestroy()
    {
        _weaponsCarousel.OnWeaponChanged -= OnWeaponChanged;
        _weaponsService.OnSelectionChanged -= RefreshWeaponsUI;
        _weaponsService.OnWeaponsChanged -= RefreshWeaponsUI;
    }

    private void OnWeaponChanged(WeaponSO weapon)
    {
        RefreshWeaponsUI();
    }

    private void SelectWeapon()
    {
        var weapon = _weaponsCarousel.CurrentWeapon;
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

        RefreshWeaponsUI();
    }

    private void RefreshWeaponsUI()
    {
        var weapon = _weaponsCarousel.CurrentWeapon;
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

        if (ChangeLockState(id))
            return;

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

    private bool ChangeLockState(int weaponId)
    {
        bool isLock = !_weaponsService.IsAvailable(weaponId);

        if (isLock)
        {
            selectButton.interactable = false;
            selectButtonText.text = "Locked";
        }

        _mainMenuUIManager.SetLockPanelState(isLock);
        upgradeButton.interactable = !isLock;

        return isLock;
    }
}
