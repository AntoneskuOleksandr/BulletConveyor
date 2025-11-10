using System.Collections.Generic;
using Zenject;

public class BasicShootStrategy : IShootStrategy
{
    [Inject] private BulletManager _bulletManager;
    [Inject] private WeaponFireController _fireController;

    public bool IsCanShoot(Weapon weapon)
    {
        if (weapon.FireCountdown > 0f) return false;
        if (weapon.Target == null) return false;

        return _bulletManager.AvailableCount >= weapon.Data.MinBulletsToShoot;
    }

    public List<Bullet> Shoot(Weapon weapon)
    {
        var firePoint = weapon.AttachmentPoints.Get(WeaponAttachmentType.FirePoint);
        if (firePoint == null)
            return null;

        var bulletData = weapon.BuildExpandedBulletInitData(firePoint);
        var firedBullets = _fireController.SpawnBullets(weapon, firePoint, bulletData);

        if (firedBullets.Count > 0)
            weapon.ResetFireCountdown();

        return firedBullets;
    }
}
