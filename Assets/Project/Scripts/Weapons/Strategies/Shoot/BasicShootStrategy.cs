using Zenject;

public class BasicShootStrategy : IShootStrategy
{
    [Inject] private BulletManager _bulletManager;

    public bool TryShoot(Weapon weapon)
    {
        if (weapon.FireCountdown > 0f) return false;
        if (weapon.Target == null) return false;

        Bullet bullet = _bulletManager.TryGetBullet(weapon);
        if (bullet == null) return false;

        bullet.SetStrategies(weapon.BulletMoveStrategy, weapon.BulletHitStrategy);
        bullet.Init(weapon.Damage, weapon.FirePoint, weapon.Target.HitTransform);

        weapon.ResetFireCountdown();
        return true;
    }
}
