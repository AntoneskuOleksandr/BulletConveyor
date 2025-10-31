using UnityEngine;

public class AoEHitStrategy : IBulletHitStrategy
{
    public void Hit(Weapon weapon, Bullet bullet, Collider other)
    {
        Collider[] hits = Physics.OverlapSphere(bullet.transform.position, weapon.Data.GetParameter(WeaponParameterType.AoERadius));
        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Enemy")) continue;

            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(bullet.Damage);
                bullet.ApplyEffects(enemy);
            }
        }
    }
}
