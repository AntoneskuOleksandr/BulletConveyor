using UnityEngine;

public class ArcMoveStrategy : IBulletMoveStrategy
{
    public void Launch(Bullet bullet, Transform target, float launchSpeed)
    {
        bullet.Rigidbody.useGravity = true;

        Vector3 dir = bullet.transform.forward;
        bullet.Rigidbody.linearVelocity = dir * launchSpeed;
        bullet.transform.rotation = Quaternion.LookRotation(dir);
    }

    public void UpdateRotation(Bullet bullet)
    {
        Vector3 velocity = bullet.Rigidbody.linearVelocity;
        if (velocity.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(velocity);
            bullet.transform.rotation = Quaternion.Slerp(
                bullet.transform.rotation,
                targetRotation,
                Time.fixedDeltaTime * 10f
            );
        }
    }
}
