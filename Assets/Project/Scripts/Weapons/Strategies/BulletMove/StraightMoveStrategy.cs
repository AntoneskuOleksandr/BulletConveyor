using UnityEngine;

public class StraightMoveStrategy : IProjectileMoveStrategy
{
    public void Launch(Weapon weapon, ProjectileBase projectile, ProjectileInitData data)
    {
        Vector3 dir = projectile.transform.forward;

        if (projectile.Rigidbody != null)
        {
            projectile.Rigidbody.isKinematic = false;
            projectile.Rigidbody.useGravity = false;
            projectile.Rigidbody.linearVelocity = dir * data.LaunchSpeed;
        }
    }

    public void Update(Weapon weapon, ProjectileBase projectile, float deltaTime)
    {
        if (projectile.Rigidbody == null)
            projectile.transform.position += projectile.transform.forward * projectile.Data.LaunchSpeed * deltaTime;
    }
}
