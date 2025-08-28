using UnityEngine;
using Zenject;

public class BulletManager
{
    private BulletsStack _bulletsStack;

    [Inject]
    public void Construct(BulletsStack bulletsStack)
    {
        _bulletsStack = bulletsStack;
    }

    public Bullet GetBullet(Weapon weapon)
    {
        if (_bulletsStack.HaveBullet)
        {
            GameObject bulletGO = _bulletsStack.GetBulletGOFromStack();

            if (bulletGO == null)
                return null; // No available bullets in the stack

            return CreateBullet(bulletGO, weapon);
        }

        return null;
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
}
