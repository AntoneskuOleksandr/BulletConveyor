using Zenject;
using UnityEngine;

public class BasicShootStrategy : IShootStrategy
{
    [Inject] private BulletManager _bulletManager;

    public void Shoot(Weapon weapon)
    {
        if (weapon.FireCountdown > 0f) return;
        if (weapon.Target == null) return;

        Bullet bullet = _bulletManager.TryGetBullet(weapon);
        if (bullet == null) return;

        bullet.SetStrategies(weapon.BulletMoveStrategy, weapon.BulletHitStrategy);
        bullet.Init(weapon.Damage, weapon.FirePoint, weapon.Target.HitTransform);

        weapon.ResetFireCountdown();
    }
}
