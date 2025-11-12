using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class ProjectileInitializer
{
    [Inject] private EffectChanceManager _effectChanceManager;
    [Inject] private EffectFactory _effectFactory;
    [Inject] private ProjectileManager _projectileManager;

    public List<ProjectileBase> InitializeProjectiles(
        Weapon weapon,
        Transform firePoint,
        IEnumerable<ProjectileInitData> projectileDataList)
    {
        List<ProjectileBase> initialized = new List<ProjectileBase>();

        foreach (var data in projectileDataList)
        {
            ProjectileBase projectile = _projectileManager.GetProjectile();
            if (projectile == null)
                break;

            projectile.transform.position = firePoint.position;
            projectile.transform.rotation = firePoint.rotation;

            projectile.Init(
                weapon,
                data,
                weapon.ProjectileMoveStrategy,
                weapon.ProjectileHitStrategy,
                _effectChanceManager,
                _effectFactory
            );

            initialized.Add(projectile);
        }

        return initialized;
    }
}
