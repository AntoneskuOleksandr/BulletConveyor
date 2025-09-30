using UnityEngine;
using UnityEngine.UI;

public class WeaponsPanel : MonoBehaviour
{
    [SerializeField] private WeaponsCarousel weaponsCarousel;

    [SerializeField] private Button nextWeaponButton;
    [SerializeField] private Button previousWeaponButton;

    private void Awake()
    {
        nextWeaponButton.onClick.AddListener(() =>
        {
            weaponsCarousel.Next();
        });
        previousWeaponButton.onClick.AddListener(() =>
        {
            weaponsCarousel.Prev();
        });
    }
}
