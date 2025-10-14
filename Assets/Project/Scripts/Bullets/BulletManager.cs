using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class BulletManager
{
    [Inject] private BulletsStack _bulletsStack;
    [Inject] private EffectChanceManager _effectChanceManager;
    [Inject] private EffectFactory _effectFactory;

    /// <summary>
    /// Получает пулю из пула и применяет к ней эффекты.
    /// Обратите внимание — здесь мы больше не задаём стратегии!
    /// </summary>
    public Bullet TryGetBullet(Weapon weapon)
    {
        if (!_bulletsStack.HaveBullet)
            return null;

        GameObject bulletGO = _bulletsStack.GetBulletGOFromStack();
        if (bulletGO == null)
            return null;

        Bullet bullet = bulletGO.GetComponent<Bullet>();
        if (bullet == null)
            bullet = bulletGO.AddComponent<Bullet>();

        // эффекты из шанс-менеджера
        var effects = _effectChanceManager.RollEffectsForWeapon(weapon);
        foreach (var type in effects)
        {
            IOnHitEffect effect = _effectFactory.CreateEffect(type);
            if (effect != null)
                bullet.AddEffect(effect);
        }

        return bullet;
    }
}
