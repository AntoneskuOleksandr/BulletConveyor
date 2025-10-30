using UnityEngine;
using Zenject;

public class MultiBarrelShootStrategy : IShootStrategy
{
    [Inject] private BulletManager _bulletManager;
    [Inject] private EffectChanceManager _effectChanceManager;
    [Inject] private EffectFactory _effectFactory;

    private int _currentBarrelIndex = 0;

    public bool TryShoot(Weapon weapon)
    {
        if (weapon.FireCountdown > 0f) return false;
        if (weapon.Target == null) return false;

        var firePoints = weapon.AttachmentPoints.GetAll(WeaponAttachmentType.FirePoint);
        if (firePoints == null || firePoints.Count == 0)
        {
            Debug.LogWarning($"{weapon.name} has no FirePoints!");
            return false;
        }

        bool shootSimultaneously = weapon.Data.GetParameter(WeaponParameterType.ShootSimultaneously) > 0.5f;

        if (shootSimultaneously)
        {
            foreach (var firePoint in firePoints)
                SpawnBullet(weapon, firePoint);
        }
        else
        {
            var firePoint = firePoints[_currentBarrelIndex];
            SpawnBullet(weapon, firePoint);

            _currentBarrelIndex = (_currentBarrelIndex + 1) % firePoints.Count;
        }

        weapon.ResetFireCountdown();
        return true;
    }

    private void SpawnBullet(Weapon weapon, Transform firePoint)
    {
        Bullet bullet = _bulletManager.TryGetBullet(weapon);
        if (bullet == null) return;

        bullet.SetStrategies(weapon.BulletMoveStrategy, weapon.BulletHitStrategy);
        bullet.Init(weapon, _effectChanceManager, _effectFactory, firePoint);
    }
}
