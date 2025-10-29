using UnityEngine;

public class StraightMoveStrategy : IBulletMoveStrategy
{
    public void Launch(Bullet bullet, Transform target, float launchSpeed)
    {
        Vector3 dir = bullet.transform.forward.normalized;

        Debug.DrawRay(bullet.transform.position, bullet.transform.forward * 5f, Color.red, 2f);
        Debug.Log($"Bullet dir: {bullet.transform.forward}");

        bullet.Rigidbody.useGravity = false;
        bullet.Rigidbody.linearVelocity = dir * launchSpeed;
    }

    public void UpdateRotation(Bullet bullet) { }
}
