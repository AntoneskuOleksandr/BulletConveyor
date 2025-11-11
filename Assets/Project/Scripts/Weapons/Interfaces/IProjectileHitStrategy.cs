using UnityEngine;

public interface IProjectileHitStrategy
{
    void Hit(Weapon weapon, ProjectileBase projectile, Collider other);
}
