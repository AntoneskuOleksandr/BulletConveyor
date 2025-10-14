using UnityEngine;
using UnityEngine.UI;

public class ShieldedEnemy : Enemy
{
    [Header("Specifics")]
    public GameObject shield;
    public float maxShieldHealth;
    public float speedWithShield;
    public float speedWithoutShield;

    [Header("UI")]
    public Image shieldBar;

    private float shieldHealth;
    private bool isShielded = true;

    public override void Start()
    {
        base.Start();
        canvas.SetActive(true);
        shieldBar.gameObject.SetActive(true);

        agent.speed = speedWithShield;
        shieldHealth = maxShieldHealth;
        shieldBar.fillAmount = 1;
    }

    public override void TakeDamage(float damage)
    {
        if (isDead)
            return;

        if (isShielded)
        {
            shieldHealth -= damage;
            shieldBar.fillAmount = shieldHealth / maxShieldHealth;

            if (shieldHealth <= 0)
            {
                //shield.GetComponent<Animator>().SetTrigger("ShieldBroken"); 
                shieldBar.gameObject.SetActive(false);
                healthBar.gameObject.SetActive(true);

                currentHealth += shieldHealth;
                healthBar.fillAmount = currentHealth / maxHealth;

                isShielded = false;
                agent.speed = speedWithoutShield;

                return;
            }
        }
        else
        {
            currentHealth -= damage;
            healthBar.fillAmount = currentHealth / maxHealth;

            if (currentHealth <= 0)
            {
                Die();
            }
        }
    }
}
