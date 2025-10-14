using UnityEngine;

public class AoEHitStrategy : IBulletHitStrategy
{
    private readonly float _radius;

    public AoEHitStrategy(float radius)
    {
        _radius = radius;
    }

    public void Hit(Bullet bullet, Collider other)
    {
        Collider[] hits = Physics.OverlapSphere(bullet.transform.position, _radius);
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
