using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class BasicShootStrategy : IShootStrategy
{
    [Inject] private BulletManager _bulletManager;
    [Inject] private EffectChanceManager _effectChanceManager;
    [Inject] private EffectFactory _effectFactory;

    public bool TryShoot(Weapon weapon, List<Bullet> spawnedBullets)
    {
        if (weapon.FireCountdown > 0f) return false;
        if (weapon.Target == null) return false;

        Bullet bullet = _bulletManager.TryGetBullet(weapon);
        if (bullet == null) return false;

        BulletInitData data = weapon.BuildBulletInitData();

        Transform firePoint = weapon.AttachmentPoints.Get(WeaponAttachmentType.FirePoint);

        bullet.SetStrategies(weapon.BulletMoveStrategy, weapon.BulletHitStrategy);
        bullet.Init(weapon, _effectChanceManager, _effectFactory, firePoint, data);
        weapon.NotifyBulletInit(bullet);

        spawnedBullets.Add(bullet);
        weapon.ResetFireCountdown();
        return true;
    }
}
