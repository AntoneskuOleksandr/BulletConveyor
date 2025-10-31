using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class BasicShootStrategy : IShootStrategy
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
        BulletInitData data = weapon.BuildBulletInitData();
        Transform firePoint = weapon.AttachmentPoints.Get(WeaponAttachmentType.FirePoint);

        Bullet bullet = _bulletManager.GetBullet();

        bullet.SetStrategies(weapon.BulletMoveStrategy, weapon.BulletHitStrategy);
        bullet.Init(weapon, _effectChanceManager, _effectFactory, firePoint, data);
        weapon.NotifyBulletInit(bullet);

        weapon.ResetFireCountdown();
        return new List<Bullet> { bullet };
    }
}
