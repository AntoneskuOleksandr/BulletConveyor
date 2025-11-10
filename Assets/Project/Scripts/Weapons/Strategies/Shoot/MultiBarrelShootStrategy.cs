using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class MultiBarrelShootStrategy : IShootStrategy
{
    [Inject] private WeaponFireController _fireController;
    [Inject] private BulletManager _bulletManager;

    private int _currentBarrelIndex;

    public bool IsCanShoot(Weapon weapon) =>
        weapon.FireCountdown <= 0f &&
        weapon.Target != null &&
        _bulletManager.AvailableCount >= weapon.Data.MinBulletsToShoot;

    public List<Bullet> Shoot(Weapon weapon)
    {
        var firePoints = weapon.AttachmentPoints.GetAll(WeaponAttachmentType.FirePoint);
        if (firePoints == null || firePoints.Count == 0)
            return null;

        var firedBullets = new List<Bullet>();
        bool shootSimultaneously = weapon.Data.GetParameter(WeaponParameterType.ShootSimultaneously) > 0.5f;

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
                var bulletData = weapon.BuildExpandedBulletInitData(fp, desiredCount);
                var fired = _fireController.SpawnBullets(weapon, fp, bulletData);

                firedBullets.AddRange(fired);
            }
        }
        else
        {
            var fp = firePoints[_currentBarrelIndex];
            int available = _bulletManager.AvailableCount;
            if (available <= 0)
                return null;

            var bulletData = weapon.BuildExpandedBulletInitData(fp, available);
            var fired = _fireController.SpawnBullets(weapon, fp, bulletData);
            firedBullets.AddRange(fired);

            _currentBarrelIndex = (_currentBarrelIndex + 1) % firePoints.Count;
        }

        if (firedBullets.Count > 0)
            weapon.ResetFireCountdown();

        return firedBullets;
    }
}
