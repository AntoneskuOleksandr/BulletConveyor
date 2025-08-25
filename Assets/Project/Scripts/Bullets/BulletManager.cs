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
        GameObject bulletGO;
        bulletGO = _bulletsStack.GetBulletGOFromStack();

        if (bulletGO == null)
            return null; // No available bullets in the stack

        return CreateBullet(bulletGO, weapon);
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
