using UnityEngine;

public class StraightMoveStrategy : IBulletMoveStrategy
{
    public void Launch(Bullet bullet, Transform target, float launchSpeed)
    {
        Vector3 dir = (target.position - bullet.transform.position).normalized;
        bullet.Rigidbody.useGravity = false;
        bullet.Rigidbody.linearVelocity = dir * launchSpeed;
    }

    public void UpdateRotation(Bullet bullet) { }
}
