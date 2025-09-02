using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class BulletManager
{
    [Inject] private BulletsStack _bulletsStack;
    [Inject] private EffectChanceManager _effectChanceManager;
    [Inject] private EffectFactory _effectFactory;

    public Bullet GetBullet(Weapon weapon)
    {
        if (!_bulletsStack.HaveBullet) return null;

        GameObject bulletGO = _bulletsStack.GetBulletGOFromStack();
        if (bulletGO == null) return null;

        Bullet bullet = CreateBullet(bulletGO, weapon);
        AddEffectsToBullet(bullet);

        return bullet;
    }

    private Bullet CreateBullet(GameObject bulletGO, Weapon weapon)
    {
        switch (weapon.BulletType)
        {
            case BulletType.Standard:
                bulletGO.AddComponent<StandardBullet>();
                break;
            case BulletType.Missile:
                bulletGO.AddComponent<Missile>();
                break;
        }
        return bulletGO.GetComponent<Bullet>();
    }

    private void AddEffectsToBullet(Bullet bullet)
    {
        List<OnHitEffectType> effectTypes = _effectChanceManager.RollEffects();

        /*        List<OnHitEffectType> effectTypes = new List<OnHitEffectType>
                {
                    OnHitEffectType.Freeze
                };*/


        foreach (var type in effectTypes)
        {
            IOnHitEffect effect = _effectFactory.CreateEffect(type);

            Debug.Log("Add Effect To Bullet: " + effect);

            if (effect != null)
                bullet.AddEffect(effect);
        }
    }
}
