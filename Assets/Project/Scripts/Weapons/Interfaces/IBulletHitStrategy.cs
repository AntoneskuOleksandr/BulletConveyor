using UnityEngine;

public interface IBulletHitStrategy
{
    void Hit(Bullet bullet, Collider other);
}
