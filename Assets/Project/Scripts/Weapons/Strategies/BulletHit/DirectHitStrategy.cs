using UnityEngine;

public class DirectHitStrategy : IProjectileHitStrategy
{
    public void Hit(Weapon weapon, ProjectileBase projectile, Collider other)
    {
        if (!other.CompareTag("Enemy")) return;

        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(projectile.Damage);
            projectile.ApplyEffects(enemy);
        }
    }
}
