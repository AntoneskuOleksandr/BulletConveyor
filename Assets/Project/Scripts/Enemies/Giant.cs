using UnityEngine;

public class Giant : Enemy
{
    [Header("Specifics")]
    public float minSize;
    public float maxSize;

    public override void Start()
    {
        base.Start();

        minSize *= transform.localScale.x;
        maxSize *= transform.localScale.x;

        currentHealth = maxHealth;
        transform.localScale = new Vector3(maxSize, maxSize, maxSize);
        agent.speed = minSpeed;
    }

    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);

        UpdateSizeAndSpeed();
    }

    private void UpdateSizeAndSpeed()
    {
        if(isDead) return;

        float healthPercentage = currentHealth / maxHealth;
        float size = Mathf.Lerp(minSize, maxSize, healthPercentage);
        float speed = Mathf.Lerp(maxSpeed, minSpeed, healthPercentage);

        transform.localScale = new Vector3(size, size, size);
        agent.speed = speed;
    }
}
