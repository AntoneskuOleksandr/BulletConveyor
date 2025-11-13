using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class ProjectileInitializer
{
    [Inject] private EffectChanceManager _effects;
    [Inject] private EffectFactory _factory;

    public List<ProjectileBase> InitializeProjectiles(
        Weapon weapon,
        Transform firePoint,
        List<ProjectileBase> projectiles,
        List<ProjectileInitData> data)
    {
        for (int i = 0; i < projectiles.Count; i++)
        {
            var p = projectiles[i];

            p.transform.position = firePoint.position;
            p.transform.rotation = firePoint.rotation;

            p.Init(
                weapon,
                data[i],
                weapon.ProjectileMoveStrategy,
                weapon.ProjectileHitStrategy,
                _effects,
                _factory
            );
        }

        return projectiles;
    }
}
