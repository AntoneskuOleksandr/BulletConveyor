using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class BasicShootStrategy : IShootStrategy
{
    [Inject] private FireDispatcher _dispatcher;
    [Inject] private ProjectileInitializer _initializer;

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

        Transform firePoint = weapon.AttachmentPoints.Get(WeaponAttachmentType.FirePoint);

        weapon.NotifyBeforeShoot();

        var data = weapon.BuildProjectileData(bullets.Count, firePoint);

        var created = _initializer.InitializeProjectiles(weapon, firePoint, bullets, data);

        weapon.OnFireCompleted(created);
    }
}
