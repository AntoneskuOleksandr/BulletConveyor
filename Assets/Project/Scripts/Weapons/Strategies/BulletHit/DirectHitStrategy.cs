using UnityEngine;

public class DirectHitStrategy : IBulletHitStrategy
{
    public void Hit(Bullet bullet, Collider other)
    {
        if (!other.CompareTag("Enemy")) return;

        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(bullet.Damage);
            bullet.ApplyEffects(enemy);
        }
    }
}
