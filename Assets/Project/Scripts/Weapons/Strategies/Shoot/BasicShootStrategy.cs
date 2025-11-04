using System.Collections.Generic;
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
        List<Bullet> firedBullets = new List<Bullet>();
        foreach (var data in weapon.BuildExpandedBulletInitData())
        {
            Bullet bullet = _bulletManager.GetBullet();
            if (bullet == null) break;

            bullet.SetStrategies(weapon.BulletMoveStrategy, weapon.BulletHitStrategy);
            bullet.Init(weapon, _effectChanceManager, _effectFactory, weapon.AttachmentPoints.Get(WeaponAttachmentType.FirePoint), data);
            weapon.NotifyBulletInit(bullet);
            firedBullets.Add(bullet);
        }

        weapon.ResetFireCountdown();
        return firedBullets;
    }
}
