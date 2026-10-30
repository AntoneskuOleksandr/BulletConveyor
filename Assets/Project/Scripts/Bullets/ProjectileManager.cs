using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class ProjectileManager
{
    [Inject] private ProjectileStack _stack;

    public int AvailableCount => _stack.projectiles.Count;

    public bool CanAllocate(int count) => AvailableCount >= count;

    public ProjectileAllocation Allocate(int min, int max)
    {
        int available = AvailableCount;

        if (available < min)
            return ProjectileAllocation.Failed;

        int toTake = Mathf.Min(available, max);
        var result = new List<ProjectileBase>(toTake);

        for (int i = 0; i < toTake; i++)
        {
            GameObject go = _stack.GetProjectileGOFromStack();
            if (go == null)
                return ProjectileAllocation.Failed;

            var projectile = go.GetComponent<ProjectileBase>() ?? go.AddComponent<ProjectileBase>();
            result.Add(projectile);
        }

        return ProjectileAllocation.SuccessWith(result);
    }
}

public struct ProjectileAllocation
{
    public bool Success { get; }
    public List<ProjectileBase> Projectiles { get; }

    private ProjectileAllocation(bool success, List<ProjectileBase> projectiles)
    {
        Success = success;
        Projectiles = projectiles;
    }

    public static ProjectileAllocation Failed =>
        new ProjectileAllocation(false, null);

    public static ProjectileAllocation SuccessWith(List<ProjectileBase> list) =>
        new ProjectileAllocation(true, list);
}
