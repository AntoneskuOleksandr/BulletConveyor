using UnityEngine;

public interface IBulletHitStrategy
{
    void Hit(Weapon weapon, Bullet bullet, Collider other);
}
