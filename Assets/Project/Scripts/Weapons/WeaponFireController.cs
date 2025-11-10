using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class WeaponFireController
{
    [Inject] private BulletManager _bulletManager;
    [Inject] private EffectChanceManager _effectChanceManager;
    [Inject] private EffectFactory _effectFactory;

    public List<Bullet> SpawnBullets(Weapon weapon, Transform firePoint, IEnumerable<BulletInitData> bulletDataList)
    {
        var bullets = new List<Bullet>();

        foreach (var data in bulletDataList)
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

        return bullets;
    }
}
