using System.Collections.Generic;
using Zenject;

public class MultiBarrelShootStrategy : IShootStrategy
{
    [Inject] private ProjectileManager _bulletManager;
    [Inject] private ProjectileInitializer _projectileInitializer;

    private int _currentBarrelIndex;

    public bool IsCanShoot(Weapon weapon) =>
        weapon.FireCountdown <= 0f &&
        weapon.Target != null &&
        _bulletManager.AvailableCount >= weapon.Data.MinProjectilesToShoot;

    public List<ProjectileBase> Shoot(Weapon weapon)
    {
        var firePoints = weapon.AttachmentPoints.GetAll(WeaponAttachmentType.FirePoint);
        if (firePoints == null || firePoints.Count == 0)
            return null;

        var firedProjectiles = new List<ProjectileBase>();
        bool shootSimultaneously = weapon.Data.GetParameter<bool>(WeaponParameterType.ShootSimultaneously);

        if (shootSimultaneously)
        {
            int barrelCount = firePoints.Count;
            int totalAvailable = _bulletManager.AvailableCount;

            int basePerBarrel = totalAvailable / barrelCount;
            int remainder = totalAvailable % barrelCount;

            for (int i = 0; i < barrelCount; i++)
            {
                if (_bulletManager.AvailableCount <= 0)
                    break;

                int desiredCount = basePerBarrel + (i < remainder ? 1 : 0);
                if (desiredCount <= 0)
                    continue;

                var fp = firePoints[i];
                var bulletData = weapon.BuildExpandedProjectileInitData(fp, desiredCount);
                var fired = _projectileInitializer.InitializeProjectiles(weapon, fp, bulletData);

                firedProjectiles.AddRange(fired);
            }
        }
        else
        {
            var fp = firePoints[_currentBarrelIndex];
            int available = _bulletManager.AvailableCount;
            if (available <= 0)
                return null;

            var bulletData = weapon.BuildExpandedProjectileInitData(fp, available);
            var fired = _projectileInitializer.InitializeProjectiles(weapon, fp, bulletData);
            firedProjectiles.AddRange(fired);

            _currentBarrelIndex = (_currentBarrelIndex + 1) % firePoints.Count;
        }

        return firedProjectiles;
    }
}
