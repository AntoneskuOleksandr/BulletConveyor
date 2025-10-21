using UnityEngine;

public interface IBulletMoveStrategy
{
    void Launch(Bullet bullet, Transform target, float launchSpeed);
    void UpdateRotation(Bullet bullet);
}
