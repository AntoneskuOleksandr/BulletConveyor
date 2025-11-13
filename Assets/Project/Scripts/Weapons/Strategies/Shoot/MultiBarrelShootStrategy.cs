using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class MultiBarrelShootStrategy : IShootStrategy
{
    [Inject] private FireDispatcher _dispatcher;
    [Inject] private ProjectileInitializer _initializer;

    private int _currentBarrelIndex;

    public bool IsReadyToRequest(Weapon weapon, int minProjectiles)
    {
        if (weapon.WaitingForFire) return false;
        if (weapon.Target == null) return false;
        if (weapon.FireCountdown > 0f) return false;

        return true;
    }

    public void RequestFire(Weapon weapon, int minProjectiles, int maxProjectiles)
    {
        weapon.WaitingForFire = true;

        float priority = weapon.Priority;

        var request = new FireRequest(
            weapon: weapon,
            shootStrategy: this,
            min: minProjectiles,
            max: maxProjectiles,
            priority: priority
        );

        _dispatcher.Enqueue(request);
    }

    public void ShootAllocated(Weapon weapon, List<ProjectileBase> bullets)
    {
        if (bullets == null || bullets.Count == 0)
            return;

        var firePoints = weapon.AttachmentPoints.GetAll(WeaponAttachmentType.FirePoint);
        if (firePoints == null || firePoints.Count == 0)
            return;

        bool shootSimultaneously =
            weapon.Data.GetParameter<bool>(WeaponParameterType.ShootSimultaneously);

        List<ProjectileBase> assigned = new List<ProjectileBase>();

        if (shootSimultaneously)
        {
            int barrelCount = firePoints.Count;
            int perBarrel = bullets.Count / barrelCount;
            int extra = bullets.Count % barrelCount;

            int index = 0;

            for (int i = 0; i < barrelCount; i++)
            {
                Transform fp = firePoints[i];

                int countForThisBarrel = perBarrel + (i < extra ? 1 : 0);
                if (countForThisBarrel == 0)
                    continue;

                List<ProjectileBase> slice = bullets.GetRange(index, countForThisBarrel);
                index += countForThisBarrel;

                var data = weapon.BuildProjectileData(slice.Count, fp);

                var created = _initializer.InitializeProjectiles(
                    weapon, fp, slice, data);

                assigned.AddRange(created);
            }
        }
        else
        {
            int barrelIndex = _currentBarrelIndex;
            if (barrelIndex >= firePoints.Count)
                barrelIndex = 0;

            Transform fp = firePoints[barrelIndex];

            var data = weapon.BuildProjectileData(bullets.Count, fp);

            var created = _initializer.InitializeProjectiles(
                weapon, fp, bullets, data);

            assigned.AddRange(created);

            _currentBarrelIndex =
                (barrelIndex + 1) % firePoints.Count;
        }

        weapon.OnFireCompleted(assigned);
    }
}
