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
            // Simultaneous shooting from all barrels
            foreach (var firePoint in firePoints)
            {
                if (available <= 0) break;
                available--;

                Bullet bullet = SpawnBullet(weapon, firePoint);
                if (bullet != null)
                    bullets.Add(bullet);
            }
        }
        else
        {
            // Sequential shooting from barrels
            if (firePoints.Count == 0)
                return null;

            var firePoint = firePoints[_currentBarrelIndex];

            Bullet bullet = SpawnBullet(weapon, firePoint);
            if (bullet != null)
                bullets.Add(bullet);

            _currentBarrelIndex = (_currentBarrelIndex + 1) % firePoints.Count;
        }

        if (bullets.Count > 0)
        {
            weapon.ResetFireCountdown();
            return bullets;
        }

        return null;
    }

    private Bullet SpawnBullet(Weapon weapon, Transform firePoint)
    {
        Bullet bullet = _bulletManager.GetBullet();
        if (bullet == null) return null;

        BulletInitData data = weapon.BuildBulletInitData();

        bullet.SetStrategies(weapon.BulletMoveStrategy, weapon.BulletHitStrategy);
        bullet.Init(weapon, _effectChanceManager, _effectFactory, firePoint, data);
        weapon.NotifyBulletInit(bullet);

        return bullet;
    }
}
