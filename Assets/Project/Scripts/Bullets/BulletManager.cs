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

        return CreateBullet(bulletGO);
    }

    private Bullet CreateBullet(GameObject bulletGO)
    {
        Bullet bullet = bulletGO.AddComponent<StandardBullet>();

        return bullet;
    }
}
