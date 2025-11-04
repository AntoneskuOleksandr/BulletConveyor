using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class MultiBarrelShootStrategy : IShootStrategy
{
    [Inject] private BulletManager _bulletManager;
    [Inject] private EffectChanceManager _effectChanceManager;
    [Inject] private EffectFactory _effectFactory;

    private int _currentBarrelIndex = 0;

    public bool IsCanShoot(Weapon weapon)
    {
        if (weapon.FireCountdown > 0f) return false;
        if (weapon.Target == null) return false;

        return _bulletManager.AvailableCount >= weapon.Data.MinBulletsToShoot;
    }

    public List<Bullet> Shoot(Weapon weapon)
    {
        List<Transform> firePoints = weapon.AttachmentPoints.GetAll(WeaponAttachmentType.FirePoint);
        if (firePoints == null || firePoints.Count == 0)
            return null;

        List<Bullet> bullets = new List<Bullet>();

        int available = _bulletManager.AvailableCount;
        int minBulletsToShoot = weapon.Data.MinBulletsToShoot;

        if (available < minBulletsToShoot)
        {
            Debug.LogWarning($"[MultiBarrelShootStrategy] Not enough bullets: have {available}, need at least {minBulletsToShoot}");
            return null;
        }

        bool shootSimultaneously = weapon.Data.GetParameter(WeaponParameterType.ShootSimultaneously) > 0.5f;

        if (shootSimultaneously)
        {
            foreach (var firePoint in firePoints)
            {
                if (_bulletManager.AvailableCount <= 0)
                    break;

                SpawnBulletsFromFirePoint(weapon, firePoint, bullets);
            }
        }
        else
        {
            var firePoint = firePoints[_currentBarrelIndex];
            SpawnBulletsFromFirePoint(weapon, firePoint, bullets);
            _currentBarrelIndex = (_currentBarrelIndex + 1) % firePoints.Count;
        }

        if (bullets.Count > 0)
        {
            weapon.ResetFireCountdown();
            return bullets;
        }

        return null;
    }

    private void SpawnBulletsFromFirePoint(Weapon weapon, Transform firePoint, List<Bullet> bullets)
    {
        foreach (var data in weapon.BuildExpandedBulletInitData())
        {
            if (_bulletManager.AvailableCount <= 0)
                break;

            Bullet bullet = _bulletManager.GetBullet();
            if (bullet == null)
                break;

            bullet.SetStrategies(weapon.BulletMoveStrategy, weapon.BulletHitStrategy);
            bullet.Init(weapon, _effectChanceManager, _effectFactory, firePoint, data);
            weapon.NotifyBulletInit(bullet);
            bullets.Add(bullet);
        }
    }
}
