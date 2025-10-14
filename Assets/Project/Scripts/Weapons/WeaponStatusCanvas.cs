using UnityEngine;
using UnityEngine.UI;

public class WeaponStatusCanvas : MonoBehaviour
{
    [Header("Health Bar")]
    [SerializeField] private GameObject healthBar;
    [SerializeField] private Image healthFillImage;

    private void OnEnable()
    {
        healthBar.SetActive(false);
    }

    public void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        healthBar.SetActive(true);
        float fillAmount = Mathf.Clamp01(currentHealth / maxHealth);
        healthFillImage.fillAmount = fillAmount;
    }
}
