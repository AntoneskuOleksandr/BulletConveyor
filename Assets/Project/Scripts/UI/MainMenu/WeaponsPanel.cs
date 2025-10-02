using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class WeaponsPanel : MonoBehaviour
{
    [Header("References")]
    [Inject] private WeaponsCarousel weaponsCarousel;
    [Inject] private UpgradeWeaponPanel upgradeWeaponPanel;

    [Header("UI Elements")]
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Button selectButton;

    [SerializeField] private Button nextWeaponButton;
    [SerializeField] private Button previousWeaponButton;

    private void OnEnable()
    {
        nextWeaponButton.onClick.AddListener(() => weaponsCarousel.Next());
        previousWeaponButton.onClick.AddListener(() => weaponsCarousel.Prev());
        upgradeButton.onClick.AddListener(() => upgradeWeaponPanel.OnUpgradeButtonClick());

        weaponsCarousel.OnWeaponChanged += upgradeWeaponPanel.SetWeapon;
    }

    private void OnDisable()
    {
        nextWeaponButton.onClick.RemoveAllListeners();
        previousWeaponButton.onClick.RemoveAllListeners();
        upgradeButton.onClick.RemoveAllListeners();

        weaponsCarousel.OnWeaponChanged -= upgradeWeaponPanel.SetWeapon;
    }
}
