using System.Collections.Generic;
using UnityEngine;
using Zenject;

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

        foreach (var type in effectTypes)
        {
            IOnHitEffect effect = CreateEffectFromType(type);
            if (effect != null)
                bullet.AddEffect(effect);
        }
    }

    // Здесь создаём конкретный IOnHitEffect по типу
    private IOnHitEffect CreateEffectFromType(OnHitEffectType type)
    {
        switch (type)
        {
            case OnHitEffectType.Fire:
                return new FireEffect(3f, 5f); // 3 сек, 5 урона/сек
            case OnHitEffectType.Freeze:
                return new FreezeEffect(0.5f, 2f); // 2 сек, -50% скорости
            // Добавляй другие эффекты по необходимости
            default:
                return null;
        }
    }
}
