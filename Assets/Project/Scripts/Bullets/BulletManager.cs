using UnityEngine;
using Zenject;

public class BulletManager
{
    [Inject] private BulletsStack _bulletsStack;

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

        return bullet;
    }
}
