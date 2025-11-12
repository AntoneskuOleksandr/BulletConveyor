using UnityEngine;

public class AoEHitStrategy : IProjectileHitStrategy
{
    public void Hit(Weapon weapon, ProjectileBase projectile, Collider other)
    {
        Collider[] hits = Physics.OverlapSphere(projectile.transform.position, weapon.Data.GetParameter<float>(WeaponParameterType.AoERadius));
        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Enemy")) continue;

            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(projectile.Damage);
                projectile.ApplyEffects(enemy);
            }
        }
    }
}
