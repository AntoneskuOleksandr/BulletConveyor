using UnityEngine;
using Zenject;

public class PeashooterShootStrategy : IShootStrategy
{
    [Inject] private BulletManager _bulletManager;
    [Inject] private EffectChanceManager _effectChanceManager;
    [Inject] private EffectFactory _effectFactory;

    private int _shotsSinceLastPower;

    public bool TryShoot(Weapon weapon)
    {
        if (weapon.FireCountdown > 0f) return false;
        if (weapon.Target == null) return false;

        var firePoints = weapon.AttachmentPoints.GetAll(WeaponAttachmentType.FirePoint);
        if (firePoints == null || firePoints.Count < 2)
        {
            Debug.LogWarning($"{weapon.name} has insufficient FirePoints for Peashooter strategy.");
            return false;
        }

        int powerEvery = Mathf.Max(1, (int)weapon.Data.GetParameter(WeaponParameterType.PowerfulShotEvery));
        float powerMultiplier = weapon.Data.GetParameter(WeaponParameterType.PowerShotMultiplier);

        Transform firePoint;
        float damageMultiplier;

        if (_shotsSinceLastPower >= powerEvery - 1)
        {
            firePoint = firePoints[1];
            damageMultiplier = powerMultiplier;
            _shotsSinceLastPower = 0;
        }
        else
        {
            firePoint = firePoints[0];
            damageMultiplier = 1f;
            _shotsSinceLastPower++;
        }

        Bullet bullet = _bulletManager.TryGetBullet(weapon);
        if (bullet == null) return false;

        bullet.SetStrategies(weapon.BulletMoveStrategy, weapon.BulletHitStrategy);

        bullet.Init(weapon, _effectChanceManager, _effectFactory, firePoint, damageMultiplier);

        weapon.ResetFireCountdown();
        return true;
    }
}