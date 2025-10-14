using UnityEngine;

public class SplittingEnemy : Enemy
{
    [Header("Specifics")]
    public GameObject enemyPrefab;
    public int splitCount;
    public float splitHealthFactor;

    public override void Die()
    {
        base.Die();

        for (int i = 0; i < splitCount; i++)
        {
            GameObject newEnemy = Instantiate(enemyPrefab, transform.position, new Quaternion(0f, 180f, 0, 0f));
            newEnemy.GetComponent<Enemy>().maxHealth *= splitHealthFactor;
            newEnemy.GetComponent<Enemy>().currentHealth = newEnemy.GetComponent<Enemy>().maxHealth;
        }
    }
}
