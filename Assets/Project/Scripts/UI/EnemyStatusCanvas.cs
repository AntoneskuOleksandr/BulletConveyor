using UnityEngine;
using UnityEngine.UI;

public class EnemyStatusCanvas : MonoBehaviour
{
    [SerializeField] private GameObject healthBar;

    public void SetHealthBarFillAmount(float fillAmount)
    {
        if (!this.gameObject.activeSelf)
            this.gameObject.SetActive(true);

        if (healthBar != null)
        {
            var fillImage = healthBar.GetComponent<Image>();
            if (fillImage != null)
            {
                fillImage.fillAmount = Mathf.Clamp01(fillAmount);
            }
        }
    }

    public void HideHealthBar()
    {
        this.gameObject.SetActive(false);
    }
}
