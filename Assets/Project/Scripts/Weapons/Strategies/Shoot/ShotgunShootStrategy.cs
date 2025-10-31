using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class ShotgunShootStrategy : IShootStrategy
{
    [Inject] private BulletManager _bulletManager;
    [Inject] private EffectChanceManager _effectChanceManager;
    [Inject] private EffectFactory _effectFactory;

    public bool TryShoot(Weapon weapon, List<Bullet> spawnedBullets)
    {
        if (weapon.FireCountdown > 0f) return false;
        if (weapon.Target == null) return false;

        Transform firePoint = weapon.AttachmentPoints.Get(WeaponAttachmentType.FirePoint);
        if (firePoint == null) return false;

        Vector3 baseDir = (weapon.Target.HitTransform.position - firePoint.position).normalized;
        int pelletCount = Mathf.RoundToInt(weapon.Data.GetParameter(WeaponParameterType.PelletCount, 1));
        float spreadAngle = weapon.Data.GetParameter(WeaponParameterType.SpreadAngle, 3f);

        for (int i = 0; i < pelletCount; i++)
        {
            Bullet bullet = _bulletManager.TryGetBullet(weapon);
            if (bullet == null) continue;

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
            spawnedBullets.Add(bullet);
        }

        weapon.ResetFireCountdown();
        return true;
    }
}
