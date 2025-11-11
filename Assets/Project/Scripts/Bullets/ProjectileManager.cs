using UnityEngine;
using Zenject;

public class ProjectileManager
{
    [Inject] private ProjectileStack _projectileStack;

    public int AvailableCount => _projectileStack.projectiles.Count;

    public ProjectileBase GetProjectile()
    {
        if (!_projectileStack.HaveProjectile)
        {
            Debug.LogError($"No projectiles available in the stack");
            return null;
        }

        GameObject projectileGO = _projectileStack.GetProjectileGOFromStack();
        if (projectileGO == null)
        {
            Debug.LogError($"Failed to get projectile GameObject from stack");
            return null;
        }

        ProjectileBase projectile = projectileGO.GetComponent<ProjectileBase>();

        if (projectile == null)
            projectile = projectileGO.AddComponent<ProjectileBase>();

        return projectile;
    }
}
