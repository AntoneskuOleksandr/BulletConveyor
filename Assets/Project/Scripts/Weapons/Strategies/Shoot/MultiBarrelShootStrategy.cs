using System.Collections.Generic;
using Zenject;

public class MultiBarrelShootStrategy : IShootStrategy
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
        var bullets = new List<Bullet>();

        var firePoints = weapon.AttachmentPoints.GetAll(WeaponAttachmentType.FirePoint);
        if (firePoints == null || firePoints.Count == 0)
            return bullets;

        foreach (var firePoint in firePoints)
        {
            Bullet bullet = _bulletManager.GetBullet();
            if (bullet == null) continue;

            BulletInitData data = weapon.BuildBulletInitData();
            bullet.SetStrategies(weapon.BulletMoveStrategy, weapon.BulletHitStrategy);
            bullet.Init(weapon, _effectChanceManager, _effectFactory, firePoint, data);
            weapon.NotifyBulletInit(bullet);

            bullets.Add(bullet);
        }

        weapon.ResetFireCountdown();
        return bullets;
    }

}
