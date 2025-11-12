using System.Collections.Generic;
using Zenject;

public class BasicShootStrategy : IShootStrategy
{
    [Inject] private ProjectileManager _projectileManager;
    [Inject] private ProjectileInitializer _projectileInitializer;

    public bool IsCanShoot(Weapon weapon)
    {
        if (weapon.FireCountdown > 0f) return false;
        if (weapon.Target == null) return false;

        return _projectileManager.AvailableCount >= weapon.Data.MinProjectilesToShoot;
    }

    public List<ProjectileBase> Shoot(Weapon weapon)
    {
        var firePoint = weapon.AttachmentPoints.Get(WeaponAttachmentType.FirePoint);
        if (firePoint == null)
            return null;

        var projectileData = weapon.BuildExpandedProjectileInitData(firePoint);
        var fired = _projectileInitializer.InitializeProjectiles(weapon, firePoint, projectileData);

        return fired;
    }
}
