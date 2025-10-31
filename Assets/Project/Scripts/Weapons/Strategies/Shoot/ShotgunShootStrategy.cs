using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class ShotgunShootStrategy : IShootStrategy
{
    [Inject] private BulletManager _bulletManager;
    [Inject] private EffectChanceManager _effectChanceManager;
    [Inject] private EffectFactory _effectFactory;

    public bool IsCanShoot(Weapon weapon)
    {
        if (weapon.FireCountdown > 0f) return false;
        if (weapon.Target == null) return false;

        return _bulletManager.AvailableCount >= weapon.Data.MinBulletsToShoot;
    }

    public List<Bullet> Shoot(Weapon weapon)
    {
        if (weapon.FireCountdown > 0f) return null;
        if (weapon.Target == null) return null;

        Transform firePoint = weapon.AttachmentPoints.Get(WeaponAttachmentType.FirePoint);
        if (firePoint == null) return null;

        Vector3 baseDir = (weapon.Target.HitTransform.position - firePoint.position).normalized;
        float spreadAngle = weapon.Data.GetParameter(WeaponParameterType.SpreadAngle, 3f);

        List<Bullet> firedBullets = new List<Bullet>();

        int pelletCount = Mathf.RoundToInt(weapon.Data.GetParameter(WeaponParameterType.PelletCount, 1));
        int minBulletsToShoot = weapon.Data.MinBulletsToShoot;
        int available = _bulletManager.AvailableCount;

        if (available < minBulletsToShoot)
        {
            Debug.LogError($"[ShotgunShootStrategy] Not enough bullets to shoot (available {available}, need at least {minBulletsToShoot})");
            return null;
        }

        int bulletsToShoot = Mathf.Min(pelletCount, available);

        for (int i = 0; i < bulletsToShoot; i++)
        {
            Bullet bullet = _bulletManager.GetBullet();
            if (bullet == null) continue;

            firedBullets.Add(bullet);

            BulletInitData data = weapon.BuildBulletInitData();

            Quaternion spreadRot = Quaternion.Euler(
                Random.Range(-spreadAngle, spreadAngle),
                Random.Range(-spreadAngle, spreadAngle),
                0f
            );

            firePoint.rotation = Quaternion.LookRotation(spreadRot * baseDir);

            bullet.SetStrategies(weapon.BulletMoveStrategy, weapon.BulletHitStrategy);
            bullet.Init(weapon, _effectChanceManager, _effectFactory, firePoint, data);
            weapon.NotifyBulletInit(bullet);
        }

        weapon.ResetFireCountdown();
        return firedBullets;
    }
}
