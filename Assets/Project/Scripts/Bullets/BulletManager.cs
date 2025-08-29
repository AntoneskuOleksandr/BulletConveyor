using System.Collections.Generic;
using UnityEngine;
using Zenject;
using static EffectChanceManager;

public class BulletManager
{
    [Inject] private BulletsStack _bulletsStack;
    [Inject] private EffectChanceManager _effectChanceManager;

    public Bullet GetBullet(Weapon weapon)
    {
        if (!_bulletsStack.HaveBullet) return null;

        GameObject bulletGO = _bulletsStack.GetBulletGOFromStack();
        if (bulletGO == null) return null;

        Bullet bullet = CreateBullet(bulletGO, weapon);
        ModifyBullet(bullet);

        return bullet;
    }

    private Bullet CreateBullet(GameObject bulletGO, Weapon weapon)
    {
        switch (weapon.BulletType)
        {
            case BulletType.Standard: bulletGO.AddComponent<StandardBullet>(); break;
            case BulletType.Missile: bulletGO.AddComponent<Missile>(); break;
        }
        return bulletGO.GetComponent<Bullet>();
    }

    private void ModifyBullet(Bullet bullet)
    {
        List<OnHitEffectType> effects = _effectChanceManager.RollEffects();
    }
}
