using UnityEngine;
using Zenject;

public class ShotgunShootStrategy : IShootStrategy
{
    [Inject] private BulletManager _bulletManager;
    [Inject] private EffectChanceManager _effectChanceManager;
    [Inject] private EffectFactory _effectFactory;

    private const int PELLET_COUNT = 2;
    private const float SPREAD_ANGLE = 15f;

    public bool TryShoot(Weapon weapon)
    {
        if (weapon.FireCountdown > 0f) return false;
        if (weapon.Target == null) return false;

        Transform firePoint = weapon.AttachmentPoints.Get(WeaponAttachmentType.FirePoint);
        if (firePoint == null) return false;

        // Базовое направление выстрела
        Vector3 baseDir = (weapon.Target.HitTransform.position - firePoint.position).normalized;

        // Создаём пули
        for (int i = 0; i < PELLET_COUNT; i++)
        {
            Bullet bullet = _bulletManager.TryGetBullet(weapon);
            if (bullet == null) continue;

            bullet.SetStrategies(weapon.BulletMoveStrategy, weapon.BulletHitStrategy);
            bullet.Init(weapon, _effectChanceManager, _effectFactory);

            // добавляем лёгкий разброс
            Quaternion spreadRot = Quaternion.Euler(
                Random.Range(-SPREAD_ANGLE, SPREAD_ANGLE),
                Random.Range(-SPREAD_ANGLE, SPREAD_ANGLE),
                0f
            );

            bullet.transform.forward = spreadRot * baseDir;
        }

        weapon.ResetFireCountdown();
        return true;
    }
}
