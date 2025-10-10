using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class BulletManager
{
    [Inject] private BulletsStack _bulletsStack;
    [Inject] private EffectChanceManager _effectChanceManager;
    [Inject] private EffectFactory _effectFactory;

    public Bullet TryGetBullet(Weapon weapon)
    {
        if (!_bulletsStack.HaveBullet) return null;

        GameObject bulletGO = _bulletsStack.GetBulletGOFromStack();
        if (bulletGO == null) return null;

        Bullet bullet = bulletGO.GetComponent<Bullet>();
        if (bullet == null) bullet = bulletGO.AddComponent<Bullet>();

        switch (weapon.BulletType)
        {
            case BulletType.Standard:
                bullet.SetStrategies(new StraightMoveStrategy(), new DirectHitStrategy());
                break;
            case BulletType.Artillery:
                float aoeRadius = weapon.Data.GetParameter(WeaponParameterType.AoERadius, 5f);
                bullet.SetStrategies(new ArcMoveStrategy(), new AoEHitStrategy(aoeRadius));
                break;
        }

        AddEffectsToBullet(bullet, weapon);
        return bullet;
    }

    private void AddEffectsToBullet(Bullet bullet, Weapon sourceWeapon)
    {
        List<OnHitEffectType> effectTypes = _effectChanceManager.RollEffectsForWeapon(sourceWeapon);

        foreach (var type in effectTypes)
        {
            IOnHitEffect effect = _effectFactory.CreateEffect(type);
            if (effect != null)
                bullet.AddEffect(effect);
        }
    }
}
