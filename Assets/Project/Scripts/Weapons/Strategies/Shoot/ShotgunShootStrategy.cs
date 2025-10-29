using UnityEngine;
using Zenject;

public class ShotgunShootStrategy : IShootStrategy
{
    [Inject] private BulletManager _bulletManager;
    [Inject] private EffectChanceManager _effectChanceManager;
    [Inject] private EffectFactory _effectFactory;

    public bool TryShoot(Weapon weapon)
    {
        if (weapon.FireCountdown > 0f) return false;
        if (weapon.Target == null) return false;

        Transform firePoint = weapon.AttachmentPoints.Get(WeaponAttachmentType.FirePoint);
        if (firePoint == null) return false;

        Vector3 baseDir = (weapon.Target.HitTransform.position - firePoint.position).normalized;

        for (int i = 0; i < weapon.Data.GetParameter(WeaponParameterType.PelletCount); i++)
        {
            Bullet bullet = _bulletManager.TryGetBullet(weapon);
            if (bullet == null) continue;

            bullet.SetStrategies(weapon.BulletMoveStrategy, weapon.BulletHitStrategy);

            float spreadAngle = weapon.Data.GetParameter(WeaponParameterType.SpreadAngle);

            Quaternion spreadRot = Quaternion.Euler(
                Random.Range(-spreadAngle, spreadAngle),
                Random.Range(-spreadAngle, spreadAngle),
                0f
            );

            bullet.Init(weapon, _effectChanceManager, _effectFactory, (spreadRot * baseDir).normalized);
        }

        weapon.ResetFireCountdown();
        return true;
    }
}
