using Zenject;

public class BasicShootStrategy : IShootStrategy
{
    [Inject] private BulletManager _bulletManager;
    [Inject] private EffectChanceManager _effectChanceManager;
    [Inject] private EffectFactory _effectFactory;

    public bool TryShoot(Weapon weapon)
    {
        if (weapon.FireCountdown > 0f) return false;
        if (weapon.Target == null) return false;

        Bullet bullet = _bulletManager.TryGetBullet(weapon);
        if (bullet == null) return false;

        bullet.SetStrategies(weapon.BulletMoveStrategy, weapon.BulletHitStrategy);
        bullet.Init(weapon, _effectChanceManager, _effectFactory);

        weapon.ResetFireCountdown();
        return true;
    }
}
