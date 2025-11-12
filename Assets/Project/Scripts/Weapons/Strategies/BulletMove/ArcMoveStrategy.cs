using UnityEngine;

public class ArcMoveStrategy : IProjectileMoveStrategy
{
    public void Launch(Weapon weapon, ProjectileBase projectile, ProjectileInitData data)
    {
        if (projectile.Rigidbody == null)
        {
            Debug.LogError("[ArcMoveStrategy] Missing Rigidbody on projectile!");
            return;
        }

        projectile.Rigidbody.isKinematic = false;
        projectile.Rigidbody.useGravity = true;
        projectile.Rigidbody.linearVelocity = projectile.transform.forward * data.LaunchSpeed;
    }

    public void Update(Weapon weapon, ProjectileBase projectile, float deltaTime)
    {
        if (projectile.Rigidbody == null)
            return;

        Vector3 velocity = projectile.Rigidbody.linearVelocity;
        if (velocity.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(velocity);
            projectile.transform.rotation = Quaternion.Slerp(
                projectile.transform.rotation,
                targetRot,
                deltaTime * 8f
            );
        }
    }
}
